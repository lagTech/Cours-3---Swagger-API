using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MVC.Models;

namespace MVC.Models
{

    /// <summary>
    /// Implementation de la class DTO pour la lecture des Post
    /// </summary>
    public class PostReadDTO
    {
        /// <summary>
        /// ID du Post
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Titre du Post
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Catégorie du Post
        /// </summary>
        public Category Category { get; init; }

        /// <summary>
        /// Usager qui a soumis le Post
        /// </summary>
        public string? User { get; init; }

        /// <summary>
        /// Nombre de Like
        /// </summary>
        public int Like { get; init; }

        /// <summary>
        /// Nombre de Dislike
        /// </summary>
        public int Dislike { get; init; }

        /// <summary>
        /// Date de création
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// URL de l'image du Post
        /// </summary>
        public string? Url { get; init; }

        public Guid? BlobImage { get; init; }  

        // Constructeur
        public PostReadDTO() { }

        public PostReadDTO(Post post)
        {
            Id = post.Id;
            Title = post.Title;
            Category = post.Category; 
            User = post.User;
            Like = post.Like;
            Dislike = post.Dislike;
            Created = post.Created;
            Url = post.Url;
            BlobImage = post.BlobImage;
        }
    }

    public class PostCreateDTO
    {
        [FromForm(Name = "title")]
        public required string Title { get; set; }

        [FromForm(Name = "category")]
        public int Category { get; set; }

        [FromForm(Name = "user")]
        public required string User { get; set; }

        [FromForm(Name = "image")]
        public required IFormFile Image { get; set; }

        public PostCreateDTO() { }

        public PostCreateDTO(string Title, int Category, string User, IFormFile Image)
        {
            this.Title = Title;
            this.Category = Category;
            this.User = User;
            this.Image = Image;
        }
    }

}
