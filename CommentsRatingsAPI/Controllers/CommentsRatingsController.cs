using CommentsRatingsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommentsRatingsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentsRatingsController : ControllerBase
    {
        private readonly ILogger<CommentsRatingsController> _logger;
        private readonly CommentsRatingsRepository _commentsRatingsRepository;
        private readonly RabbitMQService _rabbitMQService;
        private readonly StatsUpdater _statsUpdater;

        public CommentsRatingsController(
            ILogger<CommentsRatingsController> logger,
            CommentsRatingsRepository commentsRatingsRepository,
            RabbitMQService rabbitMQService,
            StatsUpdater statsUpdater)
        {
            _logger = logger;
            _commentsRatingsRepository = commentsRatingsRepository;
            _rabbitMQService = rabbitMQService;
            _statsUpdater = statsUpdater;
        }

        // Comments Endpoints

        [Authorize]
        [HttpGet("comments/{itemId}", Name = "GetCommentsByItemId")]
        public async Task<IEnumerable<Comment>> GetCommentsByItemId(string itemId)
        {
            await _statsUpdater.UpdateStatsAsync("/comments/{itemId}");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get comment by item id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "comments/itemid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            var comments = await _commentsRatingsRepository.GetCommentsByItemId(itemId);
            return comments;
        }

        [Authorize]
        [HttpPost("comments", Name = "AddComment")]
        public async Task<IActionResult> AddCommentAsync([FromBody] Comment comment)
        {
            await _statsUpdater.UpdateStatsAsync("/comments");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Add comment: " + comment,
                Timestamp = DateTime.UtcNow,
                Url = "comments",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            try
            {
                await _commentsRatingsRepository.AddCommentAsync(comment);
                return Ok(comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("comments/{commentId}", Name = "EditCommentById")]
        public async Task<IActionResult> EditCommentAsync(string commentId, [FromBody] string newContent)
        {
            await _statsUpdater.UpdateStatsAsync("/comments/{commentId}");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Edit comment by comment id: " + commentId,
                Timestamp = DateTime.UtcNow,
                Url = "comments/commentid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            try
            {
                var success = await _commentsRatingsRepository.EditCommentAsync(commentId, newContent);

                if (success)
                {
                    return Ok("Comment updated successfully");
                }
                else
                {
                    return NotFound("Comment not found");
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("comments/{commentId}", Name = "RemoveCommentById")]
        public async Task<IActionResult> RemoveCommentAsync(string commentId)
        {
            await _statsUpdater.UpdateStatsAsync("/comments/{commentId}");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Remove comment by comment id: " + commentId,
                Timestamp = DateTime.UtcNow,
                Url = "comments/commentId",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            var success = await _commentsRatingsRepository.RemoveCommentAsync(commentId);

            if (success)
            {
                return Ok("Comment removed successfully");
            }
            else
            {
                return NotFound("Comment not found");
            }
        }

        // Ratings Endpoints

        [Authorize]
        [HttpGet("ratings/{itemId}", Name = "GetRatingsByItemId")]
        public async Task<List<Rating>> GetRatingsByItemId(string itemId)
        {
            await _statsUpdater.UpdateStatsAsync("/ratings/{itemId}");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Get ratings by item id: " + itemId,
                Timestamp = DateTime.UtcNow,
                Url = "ratings/itemId",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            var ratings = await _commentsRatingsRepository.GetRatingsByItemId(itemId);
            return ratings;
        }

        [Authorize]
        [HttpPost("ratings", Name = "AddRating")]
        public async Task<IActionResult> AddRatingAsync([FromBody] Rating rating)
        {
            await _statsUpdater.UpdateStatsAsync("/ratings");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Add rating: " + rating,
                Timestamp = DateTime.UtcNow,
                Url = "ratings",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            try
            {
                await _commentsRatingsRepository.AddRatingAsync(rating);
                return Ok(rating);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("ratings/{ratingId}", Name = "EditRatingById")]
        public async Task<IActionResult> EditRatingAsync(string ratingId, [FromBody] int newValue)
        {
            await _statsUpdater.UpdateStatsAsync("/ratings/{ratingId}");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Edit rating by Id: " + ratingId,
                Timestamp = DateTime.UtcNow,
                Url = "ratings/ratingid",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            try
            {
                var success = await _commentsRatingsRepository.EditRatingAsync(ratingId, newValue);

                if (success)
                {
                    return Ok("Rating updated successfully");
                }
                else
                {
                    return NotFound("Rating not found");
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("ratings/{ratingId}", Name = "RemoveRatingById")]
        public async Task<IActionResult> RemoveRatingAsync(string ratingId)
        {
            await _statsUpdater.UpdateStatsAsync("/ratings/{ratingId}");

            _rabbitMQService.SendLog(new LoggingEntry
            {
                Message = "Remove rating by Id: " + ratingId,
                Timestamp = DateTime.UtcNow,
                Url = "ratings/ratingId",
                CorrelationId = Guid.NewGuid().ToString(),
                ApplicationName = "CommentsRatingsAPI",
                LogType = "Info"
            });

            var success = await _commentsRatingsRepository.RemoveRatingAsync(ratingId);

            if (success)
            {
                return Ok("Rating removed successfully");
            }
            else
            {
                return NotFound("Rating not found");
            }
        }
    }
}