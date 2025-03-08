using Microsoft.AspNetCore.Http.HttpResults;
using MVC.Models;

namespace MVC.Data
{
    public interface IRepository_mini
    {
        // API

        // Posts
        Task<Results<Created<PostReadDTO>, BadRequest, InternalServerError>> CreateAPIPost(Post post);
        Task<PostReadDTO?> GetPostById(Guid id);
        Task<bool> DeletePost(Guid id);
        Task<int> GetPostCount();
        Task<PostIndexViewModel> GetPostIndex(int page = 1, int pageSize = 10);
        Task<bool> IncrementPostLike(Guid postId);
        Task<bool> IncrementPostDislike(Guid postId);

        // Comments
        Task<Results<Created<CommentReadDTO>, BadRequest, InternalServerError>> CreateAPIComment(Comment comment);
        Task<CommentReadDTO?> GetCommentById(Guid id);
        Task<bool> DeleteComment(Guid id);
        Task<int> GetCommentCount();
        Task<CommentIndexViewModel> GetCommentIndex(int page = 1, int pageSize = 10);
        Task<bool> ApproveComment(Guid commentId);
        Task<bool> IncrementCommentLike(Guid commentId);
        Task<bool> IncrementCommentDislike(Guid commentId);
        


    }
}
