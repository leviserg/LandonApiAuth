using LandonApiAuth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LandonApiAuth.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetRoomsAsync();

        Task<Room> GetRoomAsync(Guid id);

        Task<PagedResults<Room>> GetPagedRoomsAsync(
            PagingOptions pagingOptions, 
            SortOptions<Room, RoomEntity> sortOptions,
            SearchOptions<Room, RoomEntity> searchOptions
        );
    }
}
