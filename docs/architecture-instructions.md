# Saved instructions

Status: saved only. Do not change the API yet. Do not build features from this file until a later instruction says to implement them.

## Decisions already made

- Vector database: any free option is acceptable. Pick one free vector database when implementation starts.
- Model API: use the Grok API. The API key stays in local settings or the GROK_API_KEY environment variable, never in committed source.
- Current work: no API changes. No new endpoints. No application code changes until the next implementation instruction.

## Hybrid architecture

A hybrid architecture separates responsibilities: use fine-tuning to alter the model's structural behavior, and RAG to supply its factual memory.

In a typical enterprise cloud environment, deploy a customized foundational model to handle the heavy lifting of domain language and formatting, while the backend dynamically injects real-time data into its prompts.

## The three-phase hybrid architecture

Divide the development lifecycle into three distinct tracks.

### 1. The Customization Track (Fine-Tuning)

Begin by curating thousands of examples of how the model should behave. This dataset does not need the latest facts; it needs structural perfection.

- The data: format conversational logs, specialized domain terminology, or precise API schemas into JSONL files. Example shape: `{"messages": [{"role": "user", "content": "..."}, {"role": "assistant", "content": "..."}]}`.
- The execution: run a fine-tuning job on a base model (like GPT-4o-mini or a Llama 3 instance) via a cloud provider. The resulting artifact is a private model endpoint.
- The goal: the model natively speaks the company's highly specialized language, or consistently outputs perfect, complex JSON structures, without needing a 2,000-word prompt instructing it how to do so.

### 2. Remaining tracks

The instruction message ended after track 1. Tracks 2 and 3 were not included. Add them here when they are provided.

## Code layout

The hybrid pipeline is not one API project. Each responsibility is a C# class library under `api/src/`, with tests under `api/tests/`. The API host only calls these libraries.

- `api/src/RagPipeline.VectorDb`: save and search embeddings. The Qdrant client lives in this library. The API only passes the cluster URL and API key.
- `api/src/RagPipeline.Rag`: turn a question plus retrieved passages into the prompt. It uses the vector library. It does not call Grok.
- `api/src/RagPipeline.History`: create chats, save messages, list history, and open one chat.
- `api/src/RagPipeline.FineTuning`: write and check JSONL behavior examples. It does not run a training job.
- Grok stays in the API host, after the RAG library has built the prompt.

## Chat product

The React screens are documented in `docs/ui.md`. The ASP.NET host is `api/src/RagPipeline.Api`. The React app is not calling it yet.

- API style, when it is built later: synchronous. The client sends a message and waits for the full answer in one response. Do not stream the answer.
- UI stack: a React project.
- Chat screen: sidebar plus conversation, matching the supplied Gemini-style screenshot.
  - Left sidebar: product name, New chat, Library, Notebooks, and the user profile. History sits under Notebooks and appears only when chats exist. Spark is removed.
  - Empty state: when there is no chat history, hide the history list and show a welcome message.
  - Main area: the open conversation, with the assistant answer shown in full.
  - Bottom composer: a text box, a plus button, a model picker, and a microphone button.
- History screen: a separate screen in the same app. It lists past chats, can search them, and opens a chat when one is selected.
- Current answers on the chat screen are local placeholders. They appear all at once. The API uses Grok. The screen is not calling the API yet.
