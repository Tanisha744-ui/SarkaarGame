using Microsoft.AspNetCore.Mvc;
using SarkaarGame.Models;
using SarkaarGame;
using System.Collections.Concurrent;

namespace SarkaarGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly SarkaarDbContext _context;

        public ChatController(SarkaarDbContext context)
        {
            _context = context;
        }

        [HttpPost("send")] // Endpoint: api/chat/send
        public IActionResult SendMessage([FromBody] ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.RoomCode) || string.IsNullOrEmpty(message.Sender) || string.IsNullOrEmpty(message.Text))
            {
                return BadRequest("Invalid message data.");
            }

            _context.ChatMessages.Add(message);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("get/{roomCode}")] // Endpoint: api/chat/get/{roomCode}
        public IActionResult GetMessages(string roomCode)
        {
            var messages = _context.ChatMessages
                .Where(m => m.RoomCode == roomCode)
                .OrderBy(m => m.Timestamp)
                .ToList();

            if (messages.Any())
            {
                return Ok(messages);
            }

            return NotFound("No messages found for the given room code.");
        }

        [HttpDelete("clear/{roomCode}")] // Endpoint: api/chat/clear/{roomCode}
        public IActionResult ClearMessages(string roomCode)
        {
            var messages = _context.ChatMessages.Where(m => m.RoomCode == roomCode).ToList();

            if (messages.Any())
            {
                _context.ChatMessages.RemoveRange(messages);
                _context.SaveChanges();
                return Ok("Chat cleared successfully.");
            }

            return NotFound("No chat found for the given room code.");
        }
    }
}