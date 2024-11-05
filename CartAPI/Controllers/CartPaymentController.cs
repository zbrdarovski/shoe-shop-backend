using CartPaymentAPI;
using CartPaymentAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace CartAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartPaymentController : ControllerBase
    {
        private readonly ILogger<CartPaymentController> _logger;
        private readonly CartPaymentRepository _cartPaymentRepository;
        private readonly RabbitMQService _rabbitMQService;

        /*
        public CartPaymentController(ILogger<CartPaymentController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _cartPaymentRepository = new CartPaymentRepository(configuration);
        }
        */

        public CartPaymentController(ILogger<CartPaymentController> logger, CartPaymentRepository cartPaymentRepository, RabbitMQService rabbitMQService)
        {
            _logger = logger;
            _cartPaymentRepository = cartPaymentRepository;
            _rabbitMQService = rabbitMQService;
        }

        // Cart Endpoints

        [Authorize]
        [HttpPost("createcart/{userId}", Name = "CreateCart")]
        public async Task<IActionResult> CreateCartAsync(string userId)
        {
            var createdCart = await _cartPaymentRepository.CreateCartAsync(userId);
            return CreatedAtRoute("GetCartById", new { cartId = createdCart.Id }, createdCart);
        }

        [Authorize]
        [HttpGet("cart/{cartId}", Name = "GetCartById")]
        public async Task<ActionResult<Cart>> GetCartByIdAsync(string cartId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Called get cart by id: " + cartId,
                Timestamp = DateTime.UtcNow,
                Url = "cart/cartid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });

            var cart = await _cartPaymentRepository.GetCartByIdAsync(cartId);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        [Authorize]
        [HttpGet("cart/user/{userId}", Name = "GetCartByUserId")]
        public async Task<ActionResult<Cart>> GetCartByUserIdAsync(string userId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Called get user by id: " + userId,
                Timestamp = DateTime.UtcNow,
                Url = "cart/user/userid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });

            var cart = await _cartPaymentRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        [Authorize]
        [HttpPut("carts/edit/{cartId}", Name = "EditCartById")]
        public async Task<IActionResult> EditCartAsync(string cartId, [FromBody] Cart updatedCart)
        {

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Edit cart by id: " + cartId,
                Timestamp = DateTime.UtcNow,
                Url = "cart/edit/cartid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });

            var success = await _cartPaymentRepository.EditCartAsync(cartId, updatedCart);

            if (success)
            {
                return Ok("Cart updated successfully");
            }
            else
            {
                return NotFound("Cart not found");
            }
        }

        [Authorize]
        [HttpPost("cart/{cartId}/additem", Name = "AddCartItem")]
        public async Task<IActionResult> AddCartItemAsync(string cartId, [FromBody] InventoryItem cartItem)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Add item to cart: " + cartId,
                Timestamp = DateTime.UtcNow,
                Url = "cart/cartid/additem",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });

            await _cartPaymentRepository.AddCartItemAsync(cartId, cartItem);
            return Ok();
        }

        [Authorize]
        [HttpDelete("cart/{cartId}/removeitem/{cartItemId}", Name = "RemoveCartItemById")]
        public async Task<IActionResult> RemoveCartItemAsync(string cartId, string cartItemId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Remove cart item by id: " + cartId,
                Timestamp = DateTime.UtcNow,
                Url = "cart/cartid/removeitem/itemid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });
            await _cartPaymentRepository.RemoveCartItemAsync(cartId, cartItemId);
            return Ok();
        }

        [Authorize]
        [HttpDelete("cart/{cartId}", Name = "DeleteCart")]
        public async Task<IActionResult> DeleteCartAsync(string cartId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Delete cart by cart id: " + cartId,
                Timestamp = DateTime.UtcNow,
                Url = "cart/cartid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });
            await _cartPaymentRepository.DeleteCartByIdAsync(cartId);
            return Ok();
        }

        // Payment Endpoints

        [Authorize]
        [HttpGet("payments/{userId}", Name = "GetPaymentsByUserId")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByUserId(string userId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get payments by user id: " + userId,
                Timestamp = DateTime.UtcNow,
                Url = "payments/cartid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });
            try
            {
                var payments = await _cartPaymentRepository.GetPaymentsByUserIdAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving payments: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("payments", Name = "GetAllPayments")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments()
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get all payments",
                Timestamp = DateTime.UtcNow,
                Url = "payments",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });
            try
            {
                var payments = await _cartPaymentRepository.GetAllPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving payments: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("payment/add", Name = "AddPayment")]
        public async Task<IActionResult> AddPaymentAsync([FromBody] Payment payment)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Add payment",
                Timestamp = DateTime.UtcNow,
                Url = "payment/add",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });
            try
            {
                // Set necessary properties like PaymentDate, PaymentId, etc.
                //payment.PaymentDate = DateTime.Now;
                //payment.Id = Guid.NewGuid().ToString();

                await _cartPaymentRepository.AddPaymentAsync(payment);

                // Optionally, you can save the cart, reset it, or perform other actions here

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("payment/remove/{paymentId}", Name = "DeletePaymentById")]
        public async Task<IActionResult> DeletePaymentByIdAsync(string paymentId)
        {
            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Delete payment by id: " + paymentId,
                Timestamp = DateTime.UtcNow,
                Url = "payment/remove/paymentid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CartPaymentAPI",
                LogType = "Info"
            });
            await _cartPaymentRepository.DeletePaymentByIdAsync(paymentId);
            return Ok();
        }
    }
}
