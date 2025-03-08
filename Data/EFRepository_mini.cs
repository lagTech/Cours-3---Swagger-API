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


        //Posts
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

        public async Task<PostIndexViewModel> GetPostIndex(int page = 1, int pageSize = 10)
        {
            int totalPosts = await _context.Posts.CountAsync();

            var posts = await _context.Posts
                .OrderByDescending(p => p.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostReadDTO(p))  // ✅ Now contains BlobImage
                .ToListAsync();

            return new PostIndexViewModel
            {
                Posts = posts.Select(p =>
                {
                    var post = new Post
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Category = p.Category,
                        User = p.User,
                        Created = p.Created,
                        Url = p.Url,
                        BlobImage = p.BlobImage  // ✅ Now it works!
                    };

                    for (int i = 0; i < p.Like; i++) post.IncrementLike();
                    for (int i = 0; i < p.Dislike; i++) post.IncrementDislike();

                    return post;
                }).ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
                PageSize = pageSize
            };
        }

        public async Task<int> GetPostCount()
        {
            return await _context.Posts.CountAsync();
        }

        public async Task<bool> IncrementPostLike(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            post.IncrementLike();  // Increment like count
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> IncrementPostDislike(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return false;

            post.IncrementDislike();  // Increment dislike count
            await _context.SaveChangesAsync();
            return true;
        }


        //Comments
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

        public async Task<CommentReadDTO?> GetCommentById(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            return comment != null ? new CommentReadDTO(comment) : null;
        }

        public async Task<bool> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CommentIndexViewModel> GetCommentIndex(int page = 1, int pageSize = 10)
        {
            int totalComments = await _context.Comments.CountAsync();
            var comments = await _context.Comments
                .OrderByDescending(c => c.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentReadDTO(c))
                .ToListAsync();
            return new CommentIndexViewModel
            {
                Comments = comments.Select(c =>
                {
                    var comment = new Comment
                    {
                        Id = c.Id,
                        Commentaire = c.Commentaire,
                        User = c.User,
                        Created = c.Created,
                    };
                    for (int i = 0; i < c.Like; i++) comment.IncrementLike();
                    for (int i = 0; i < c.Dislike; i++) comment.IncrementDislike();
                    return comment;
                }).ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalComments / (double)pageSize),
                PageSize = pageSize
            };
        }

        public async Task<int> GetCommentCount()
        {
            return await _context.Comments.CountAsync();
        }

        public async Task<bool> ApproveComment(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;
            comment.Approve();  // Approve comment
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementCommentLike(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            comment.IncrementLike();  // Increment like count
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementCommentDislike(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            comment.IncrementDislike() ;  // Increment dislike count
            await _context.SaveChangesAsync();
            return true;
        }

    }
}