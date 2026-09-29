using Microsoft.AspNetCore.Mvc;
using RagPipeline.Rag;
using RagPipeline.VectorDb;

namespace RagPipeline.Api.Controllers;

[ApiController]
[Route("documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IVectorStore _vectors;
    private readonly IEmbeddingGenerator _embeddings;

    public DocumentsController(IVectorStore vectors, IEmbeddingGenerator embeddings)
    {
        _vectors = vectors;
        _embeddings = embeddings;
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] UploadDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Document text is required." });
        }

        // Extremely simple chunking: split by double newlines, or paragraphs
        var chunks = request.Text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(c => c.Trim())
                                 .Where(c => c.Length > 0)
                                 .ToList();
                                 
        if (chunks.Count == 0)
        {
            chunks.Add(request.Text.Trim());
        }

        foreach (var chunk in chunks)
        {
            var embedding = _embeddings.Embed(chunk);
            var documentId = Guid.NewGuid().ToString("N");
            var doc = new VectorDocument(documentId, chunk, embedding);
            await _vectors.UpsertAsync(doc, cancellationToken);
        }

        return Ok(new { message = $"Successfully ingested {chunks.Count} chunks into the vector database." });
    }
}

public sealed record UploadDocumentRequest(string Text);
