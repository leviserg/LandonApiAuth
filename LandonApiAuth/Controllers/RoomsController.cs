using LandonApiAuth.Infrastructure;
using LandonApiAuth.Models;
using LandonApiAuth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApiAuth.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IOpeningService _openingService;
        private readonly IDateLogicService _dateLogicService;
        private readonly IBookingService _bookingService;
        private readonly PagingOptions _defaultPagingOptions;

        public RoomsController(
            IRoomService roomService,
            IOpeningService openingService,
            IDateLogicService dateLogicService,
            IBookingService bookingService,
            IOptions<PagingOptions> defaultPagingOptionsWrapper)
        {
            _roomService = roomService;
            _openingService = openingService;
            _dateLogicService = dateLogicService;
            _bookingService = bookingService;
            _defaultPagingOptions = defaultPagingOptionsWrapper.Value;
        }

        // GET /rooms?offet=1&limit=2&orderBy=rate desc
        [HttpGet(Name = nameof(GetAllRooms))]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Collection<Room>>> GetAllRooms(
            [FromQuery] PagingOptions pagingOptions = null,
            [FromQuery] SortOptions<Room, RoomEntity> sortOptions = null,
            [FromQuery] SearchOptions<Room,RoomEntity> searchOptions = null)
        {            
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var rooms = await _roomService.GetPagedRoomsAsync(
                pagingOptions, sortOptions, searchOptions);

            var collection = PagedCollection<Room>.Create<RoomsResponse>(
                Link.ToCollection(nameof(GetAllRooms)),
                rooms.Items.ToArray(),
                rooms.TotalSize,
                pagingOptions);

            collection.Openings = Link.ToCollection(nameof(GetAllRoomOpenings));
            collection.RoomsQuery = FormMetadata.FromResource<Room>(
                Link.ToForm(
                    nameof(GetAllRooms),
                    null,
                    Link.GetMethod,
                    Form.QueryRelation));

            return collection;
        }

        // GET /rooms/{roomId}
        [HttpGet("{roomId}", Name = nameof(GetRoomById))]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Room>> GetRoomById(Guid roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();

            return room;
        }

        // GET /rooms/openings/{?offset=1&limit=2&orderBy=name(rate,startAt,endAt){ desc}}
        /*
            Search functionality:
                    ?search={field} {operator: [eq (numeric, string), 
                        lt (numeric, date), lte, gt, gte],
                        sw (startsWith), co (contains) - for String Search Pattern
                        } {value}
                    + multiple search:
                    GET /rooms/openings?search=rate lt 199.99&search=startAt gte 2022-01-10
         */
        [HttpGet("openings", Name = nameof(GetAllRoomOpenings))]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<Collection<Opening>>> GetAllRoomOpenings(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Opening, OpeningEntity> sortOptions,
            [FromQuery] SearchOptions<Opening, OpeningEntity> searchOptions)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var openings = await _openingService.GetOpeningsAsync(
                pagingOptions, sortOptions, searchOptions);

            var collection = PagedCollection<Opening>.Create<OpeningsResponse>(
                Link.ToCollection(nameof(GetAllRoomOpenings)),
                openings.Items.ToArray(),
                openings.TotalSize,
                pagingOptions);

            collection.OpeningsQuery = FormMetadata.FromResource<Opening>(
                Link.ToForm(
                    nameof(GetAllRoomOpenings),
                    null,
                    Link.GetMethod,
                    Form.QueryRelation));

            return collection;
        }

        // POST /rooms/{roomId}/bookings
        // TODO authentication!
        [HttpPost("{roomId}/bookings", Name = nameof(CreateBookingForRoom))]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(201)]
        public async Task<ActionResult> CreateBookingForRoom(
            Guid roomId, [FromBody] BookingForm bookingForm)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            if (room == null) return NotFound();

            var minimumStay = _dateLogicService.GetMinimumStay();
            bool tooShort = (bookingForm.EndAt.Value - bookingForm.StartAt.Value) < minimumStay;
            if (tooShort) return BadRequest(new ApiError(
                $"The minimum booking duration is {minimumStay.TotalHours} hours."));
            /*
            var conflictedSlots = await _openingService.GetConflictingSlots(
                roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value);
            if (conflictedSlots.Any()) return BadRequest(new ApiError(
                "This time conflicts with an existing booking."));
            */
            // Get the current user (TODO)
            var userId = Guid.NewGuid();

            var bookingId = await _bookingService.CreateBookingAsync(
                userId, roomId, bookingForm.StartAt.Value, bookingForm.EndAt.Value);

            return Created(
                Url.Link(nameof(BookingsController.GetBookingById),
                new { bookingId }),
                null);
        }
    }
}
