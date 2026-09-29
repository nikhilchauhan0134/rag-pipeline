using Microsoft.AspNetCore.Mvc;
using RagPipeline.Rag;
using RagPipeline.VectorDb;
using UglyToad.PdfPig;

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
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided." });
        }

        string text;

        if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var document = PdfDocument.Open(stream);
                var pagesText = document.GetPages().Select(p => p.Text);
                text = string.Join("\n\n", pagesText);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to parse PDF.", details = ex.Message });
            }
        }
        else
        {
            using var reader = new StreamReader(file.OpenReadStream());
            text = await reader.ReadToEndAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest(new { error = "The uploaded document contained no readable text." });
        }

        // Extremely simple chunking: split by double newlines, or paragraphs
        var chunks = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(c => c.Trim())
                         .Where(c => c.Length > 0)
                         .ToList();
                                 
        if (chunks.Count == 0)
        {
            chunks.Add(text.Trim());
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
