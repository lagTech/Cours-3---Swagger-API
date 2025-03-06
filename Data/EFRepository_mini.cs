using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepository_mini<TContext> : IRepository_mini where TContext : ApplicationDbContextInMemory
    {
        protected readonly TContext _context;

        protected EFRepository_mini(TContext context)
        {
            this._context = context;
        }

        public virtual async Task<Results<Created<PostReadDTO>, BadRequest, InternalServerError>> CreateAPIPost(Post post)
        {
            try
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return TypedResults.Created($"/Posts/{post.Id}", new PostReadDTO(post));

            }
            catch (Exception ex) when (ex is DbUpdateException)
            {
                return TypedResults.BadRequest();
            }
            catch (Exception)
            {
                return TypedResults.InternalServerError();
            }
        }

        public async Task<IEnumerable<PostReadDTO>> GetPosts()
        {
            return await _context.Posts.Select(post => new PostReadDTO(post)).ToListAsync();
        }

        public async Task<PostReadDTO?> GetPostById(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            return post != null ? new PostReadDTO(post) : null;
        }

        public async Task<bool> DeletePost(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<Results<Created<CommentReadDTO>, BadRequest, InternalServerError>> CreateAPIComment(Comment comment)
        {
            try
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return TypedResults.Created($"/Comments/{comment.Id}", new CommentReadDTO(comment));
            }
            catch (Exception ex) when (ex is DbUpdateException)
            {
                return TypedResults.BadRequest();
            }
            catch (Exception)
            {
                return TypedResults.InternalServerError();
            }
        }
    }
}