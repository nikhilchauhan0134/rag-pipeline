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
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string? chatId, CancellationToken cancellationToken)
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

        var chunks = new List<string>();
        int chunkSize = 2000;
        int overlap = 200;

        for (int i = 0; i < text.Length; i += chunkSize - overlap)
        {
            int length = Math.Min(chunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length));
            if (i + length >= text.Length) break;
        }

        foreach (var chunk in chunks)
        {
            var embedding = _embeddings.Embed(chunk);
            var documentId = Guid.NewGuid().ToString("N");
            var doc = new VectorDocument(documentId, chatId ?? "", chunk, embedding);
            await _vectors.UpsertAsync(doc, cancellationToken);
        }

        return Ok(new { message = $"Successfully ingested {chunks.Count} chunks into the vector database." });
    }
}
