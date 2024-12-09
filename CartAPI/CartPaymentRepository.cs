using CartPaymentAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CartPaymentAPI
{
    public class CartPaymentRepository
    {
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly IMongoCollection<Payment> _paymentCollection;

        public CartPaymentRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("cartpayment");
            _cartCollection = database.GetCollection<Cart>("cart");
            _paymentCollection = database.GetCollection<Payment>("payment");
        }

        public async Task<Cart> CreateCartAsync(string userId)
        {
            var newCart = new Cart
            {
                Id = userId,
                UserId = userId,
                CartItems = new List<InventoryItem>(),
                CartAmount = 0
            };

            await _cartCollection.InsertOneAsync(newCart);
            return newCart;
        }


        public async Task<Cart> GetCartByIdAsync(string cartId)
        {
            return await _cartCollection.Find(cart => cart.Id == cartId).FirstOrDefaultAsync();
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _cartCollection.Find(cart => cart.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<bool> EditCartAsync(string cartId, Cart updatedCart)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("CartId cannot be null or empty.");
            }

            var filter = Builders<Cart>.Filter.Eq(cart => cart.Id, cartId);
            var result = await _cartCollection.ReplaceOneAsync(filter, updatedCart);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task DeleteCartByIdAsync(string cartId)
        {
            await _cartCollection.DeleteOneAsync(cart => cart.Id == cartId);
        }

        public async Task AddCartItemAsync(string cartId, InventoryItem cartItem)
        {
            var filter = Builders<Cart>.Filter.Eq(cart => cart.Id, cartId);
            var update = Builders<Cart>.Update.Push(cart => cart.CartItems, cartItem);
            await _cartCollection.UpdateOneAsync(filter, update);

            // Update the CartAmount when adding an item
            await RecalculateCartAmountAsync(cartId);
        }

        public async Task RemoveCartItemAsync(string cartId, string cartItemId)
        {
            var filter = Builders<Cart>.Filter.Eq(cart => cart.Id, cartId);
            var update = Builders<Cart>.Update.PullFilter(cart => cart.CartItems, item => item.Id == cartItemId);
            await _cartCollection.UpdateOneAsync(filter, update);

            // Update the CartAmount when removing an item
            await RecalculateCartAmountAsync(cartId);
        }

        public async Task<List<Payment>> GetPaymentsByUserIdAsync(string userId)
        {
            var payments = await _paymentCollection.Find(payment => payment.UserId == userId).ToListAsync();
            return payments;
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            var payments = await _paymentCollection.Find(_ => true).ToListAsync();
            return payments;
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            // Check if InventoryItems list is not empty and CartAmount to pay is not 0
            if (payment.InventoryItems == null || payment.InventoryItems.Count == 0 || payment.Amount == 0)
            {
                throw new ArgumentException("Invalid payment data.");
            }

            // Insert payment into the payment collection
            await _paymentCollection.InsertOneAsync(payment);

            // Reset the cart after payment (move items to payment section)
            await ResetCartAsync(payment.CartId);
        }

        public async Task DeletePaymentByIdAsync(string paymentId)
        {
            // Delete payment by id
            await _paymentCollection.DeleteOneAsync(payment => payment.Id == paymentId);
        }

        public async Task ResetCartAsync(string cartId)
        {
            var cart = await _cartCollection.Find(c => c.Id == cartId).FirstOrDefaultAsync();
            if (cart != null)
            {
                // Check if CartItems list is not empty and CartAmount is not 0
                if (cart.CartItems != null && cart.CartItems.Count > 0 && cart.CartAmount != 0)
                {
                    var payment = new Payment
                    {
                        CartId = cart.Id,
                        UserId = cart.UserId,
                        Amount = cart.CartAmount ?? 0,
                        InventoryItems = cart.CartItems,
                        PaymentDate = DateTime.Now
                    };

                    // Insert payment into the payment collection
                    await _paymentCollection.InsertOneAsync(payment);

                    // Empty the items in the cart
                    var update = Builders<Cart>.Update.Set(c => c.CartItems, new List<InventoryItem>());
                    await _cartCollection.UpdateOneAsync(c => c.Id == cartId, update);

                    // Reset the CartAmount
                    await UpdateCartAmountAsync(cartId, 0);
                }
            }
        }

        private async Task RecalculateCartAmountAsync(string cartId)
        {
            var cart = await _cartCollection.Find(c => c.Id == cartId).FirstOrDefaultAsync();
            if (cart != null)
            {
                var cartAmount = cart.CartItems?.Sum(item => item.Price * (item.Quantity ?? 1)) ?? 0;
                await UpdateCartAmountAsync(cartId, cartAmount);
            }
        }

        private async Task UpdateCartAmountAsync(string cartId, double cartAmount)
        {
            var filter = Builders<Cart>.Filter.Eq(cart => cart.Id, cartId);
            var update = Builders<Cart>.Update.Set(cart => cart.CartAmount, cartAmount);
            await _cartCollection.UpdateOneAsync(filter, update);
        }
    }
}
