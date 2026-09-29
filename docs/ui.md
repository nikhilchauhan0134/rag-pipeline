# UI

The chat screen is a React app in `ui/`. It does not call the C# libraries directly. The ASP.NET Web API in `api/src/RagPipeline.Api` is the HTTP host. The React app is not wired to it yet.

Run the screen locally:

```powershell
cd ui
npm install
npm run dev
```

Open http://localhost:5173/.

## Screens

The app has two screens. The sidebar stays on both.

### Chat

Shown on first load, after New chat, and when a past chat is opened.

- No chats yet: the main area shows **Welcome** and “No chats yet. Ask a question to begin.” History is hidden.
- Chats exist, but none is open: **Welcome** says to start a new chat or open one from History.
- A chat is open: the question and the full answer are shown above the composer. The answer is not streamed. The screen waits, shows “Writing the full answer…”, then shows the complete reply.

The composer has a plus button, the message box, a Pro / Fast model picker, a microphone button, and Send. Send is disabled while an answer is in progress.

Today the screen answer is still a local placeholder. The API calls Grok when a message is sent to the API. The screen is not calling that API yet.

### History

History appears under Notebooks only after at least one chat exists. Click **History** to open the list. The list has its own scrollbar. Search filters by title or message text. Choosing a row opens that chat.

## Sidebar

| Control | What it does now |
|---|---|
| Collapse | Hides the sidebar labels |
| New chat | Clears the open conversation and returns to Welcome |
| Library | Visible only. No screen yet |
| New notebook | Adds “Untitled notebook” in the sidebar only |
| Notebook row | Visible only. Does not open a notebook |
| History | Opens the history screen when chats exist |
| Chat title | Opens that chat |
| Profile and settings | Visible only. The name shown is Nikhil Chauhan |

## How the UI will call the API

The React app will keep calling HTTP. It does not reference the C# projects. `RagPipeline.Api` references the libraries and calls Grok.

Run the API:

```powershell
cd api/src/RagPipeline.Api
dotnet run
```

It listens on http://localhost:8000. The Grok key is read from local settings or `GROK_API_KEY`. Pro uses `grok-4`. Fast uses `grok-4-fast`.

| UI action | Request | Library behind it |
|---|---|---|
| New chat | `POST /chats` | History |
| Send | `POST /chats/{id}/messages` | History saves the question. Rag builds the prompt from VectorDb. The API calls Grok, then History saves the full answer. |
| History list | `GET /chats` | History |
| Search history | `GET /chats?q=` | History |
| Open a chat | `GET /chats/{id}` | History |

Send stays synchronous: one request, one complete answer. Pro or Fast is a field on that request, not a separate API.

Fine-tuning is not part of this screen. Library, notebooks, the plus button, the microphone, and settings need their own APIs only when those controls do real work.
