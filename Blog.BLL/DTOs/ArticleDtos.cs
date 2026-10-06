using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.BLL.DTOs
{
    public record ArticleDto(
        int Id,
        string Title,
        string Content,
        DateTime CreatedAt,
        string AuthorId,
        string AuthorFullName,
        List<string> Tags,
        int CommentsCount
    );

    public record ArticleDetailsDto(
        int Id,
        string Title,
        string Content,
        DateTime CreatedAt,
        string AuthorId,
        string AuthorFullName,
        List<string> Tags,
        List<CommentDto> Comments
    );

    public record CreateArticleDto(
        string Title,
        string Content,
        string AuthorId,
        List<string> Tags
    );

    public record EditArticleDto(
        int Id,
        string Title,
        string Content,
        string UserId,
        List<string> Tags
    );

    public record CommentDto(
        int Id,
        string Content,
        DateTime CreatedAt,
        string AuthorFullName
    );

    public record CreateCommentDto(
        int ArticleId,
        string Content,
        string AuthorId
    );
}
