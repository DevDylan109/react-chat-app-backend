// using Microsoft.EntityFrameworkCore;
// using react_chat_app_backend.Context;
// using react_chat_app_backend.Models;
// using react_chat_app_backend.Repositories.Interfaces;
//
// namespace react_chat_app_backend.Repositories;
//
// public class ChatTabRepository : IChatTabRepository
// {
//     private AppDbContext _appDbContext;
//
//     public ChatTabRepository(AppDbContext appDbContext)
//     {
//         _appDbContext = appDbContext;
//     }
//
//     public async Task<ChatTab> GetChatTab(string UserId)
//     {
//         return await _appDbContext.ChatTabs.FirstOrDefaultAsync(ct => ct.UserId == UserId);
//     }
//
//     public async Task CreateChatTab(string UserId, int unreadMessageCount, bool isHighlighted)
//     {
//         await _appDbContext.AddAsync(new ChatTab
//         {
//             UserId = UserId,
//             isHighlighted = isHighlighted,
//             unreadMessageCount = unreadMessageCount
//         });
//         await _appDbContext.SaveChangesAsync();
//     }
//
//     public async Task RemoveChatTab(string UserId)
//     {
//         var chatTab = await GetChatTab(UserId);
//         _appDbContext.Remove(chatTab);
//         await _appDbContext.SaveChangesAsync();
//     }
// }