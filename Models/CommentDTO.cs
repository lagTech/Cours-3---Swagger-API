using MVC.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MVC.Models
{
    // Implementation du DTO
    public class CommentReadDTO
    {
        public Guid Id { get; init; }

        public string Commentaire { get; init; }

        public string User { get; init; }

        public int Like { get; init; } 

        public int Dislike { get; init; }

        public DateTime Created { get; init; }

        public bool IsApproved { get; init; }

        public Guid PostId { get; init; }

        public CommentReadDTO(Comment comment)
        {
            Id = comment.Id;
            Commentaire = comment.Commentaire;
            User = comment.User;
            Like = comment.Like;
            Dislike = comment.Dislike;
            Created = comment.Created;
            PostId = comment.PostId;
            IsApproved = comment.IsApproved;
        }
    }

    public class CommentCreateDTO
    {
        [FromForm(Name = "comment")]
        public string? Commentaire { get; init; }

        [FromForm(Name = "user")]
        public string? User { get; init; }

        [FromForm(Name = "post id")]
        public required Guid PostId { get; init; }

        public CommentCreateDTO() { }

        public CommentCreateDTO(Comment comment)
        {
            Commentaire = comment.Commentaire;
            User = comment.User;
            PostId = comment.PostId;
        }

        public static Comment GetComment(CommentCreateDTO comment)
        {
            return new Comment { Commentaire = comment.Commentaire!, User = comment.User!, PostId = comment.PostId }; 
        }
    }
}
