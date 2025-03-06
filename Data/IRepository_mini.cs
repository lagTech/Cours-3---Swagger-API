using Microsoft.AspNetCore.Http.HttpResults;
using MVC.Models;

namespace MVC.Data
{
    public interface IRepository_mini
    {
        // API
        Task<Results<Created<PostReadDTO>, BadRequest, InternalServerError>> CreateAPIPost(Post post);
        Task<IEnumerable<PostReadDTO>> GetPosts();
        Task<PostReadDTO?> GetPostById(Guid id);
        Task<bool> DeletePost(Guid id);
        Task<Results<Created<CommentReadDTO>, BadRequest, InternalServerError>> CreateAPIComment(Comment comment);

    }
}
