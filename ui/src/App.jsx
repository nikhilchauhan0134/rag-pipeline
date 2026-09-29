import { useMemo, useState, useEffect } from "react";
import { initialNotebooks } from "./sampleData";

const API_BASE = "http://localhost:8000";

export default function App() {
  const [chats, setChats] = useState([]);
  const [notebooks, setNotebooks] = useState(initialNotebooks);
  const [activeChatId, setActiveChatId] = useState(null);
  const [screen, setScreen] = useState("chat");
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [draft, setDraft] = useState("");
  const [pending, setPending] = useState(false);
  const [historyQuery, setHistoryQuery] = useState("");
  const [model, setModel] = useState("Pro");
  const [uploadedFile, setUploadedFile] = useState(null);
  const [uploading, setUploading] = useState(false);

  // Load history on mount
  useEffect(() => {
    fetch(`${API_BASE}/chats`)
      .then((res) => res.json())
      .then((data) => setChats(data))
      .catch((err) => console.error("Failed to load history", err));
  }, []);

  const activeChat = chats.find((chat) => chat.id === activeChatId) ?? null;

  const history = useMemo(() => {
    const query = historyQuery.trim().toLowerCase();
    if (!query) return chats;
    return chats.filter((chat) => {
      const blob = [chat.title, chat.updatedAt, ...chat.messages.map((message) => message.content)]
        .join(" ")
        .toLowerCase();
      return blob.includes(query);
    });
  }, [chats, historyQuery]);

  function openChat(id) {
    setActiveChatId(id);
    setUploadedFile(null);
    setScreen("chat");
  }

  function openHistory() {
    if (chats.length === 0) {
      setScreen("chat");
      return;
    }
    setScreen("history");
  }

  function startNewChat() {
    setActiveChatId(null);
    setUploadedFile(null);
    setDraft("");
    setScreen("chat");
  }

  function addNotebook() {
    const count = notebooks.length + 1;
    setNotebooks((current) => [
      ...current,
      { id: `notebook-${count}`, title: count === 1 ? "Untitled notebook" : `Untitled notebook ${count}` },
    ]);
  }

  async function sendMessage() {
    const text = draft.trim();
    if (!text || pending) return;

    setPending(true);
    let chatId = activeChatId;

    try {
      // 1. Create chat if this is a new conversation
      if (!chatId) {
        const title = text.length > 42 ? `${text.slice(0, 42)}...` : text;
        const res = await fetch(`${API_BASE}/chats`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ title }),
        });
        const newChat = await res.json();
        chatId = newChat.id;
        setActiveChatId(chatId);
        setChats((current) => [newChat, ...current]);
      }

      // Optimistically add user message
      const tempUserMsg = { id: `temp-${Date.now()}`, role: "user", content: text };
      setChats((current) =>
        current.map((chat) =>
          chat.id === chatId
            ? { ...chat, messages: [...chat.messages, tempUserMsg] }
            : chat
        )
      );
      setDraft("");

      // 2. Send the message to the API
      const res = await fetch(`${API_BASE}/chats/${chatId}/messages`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ content: text, model }),
      });

      if (!res.ok) {
        throw new Error("Failed to send message");
      }

      const data = await res.json();
      
      // Update with the full chat returned by the server
      setChats((current) =>
        current.map((chat) => (chat.id === chatId ? data.chat : chat))
      );
    } catch (err) {
      console.error("Error sending message:", err);
      alert("Failed to get an answer from the API.");
    } finally {
      setPending(false);
    }
  }

  async function handleFileUpload(event) {
    const file = event.target.files[0];
    if (!file) return;

    setUploading(true);
    try {
      let currentChatId = activeChatId;
      
      // If there's no active chat, create one first before uploading!
      if (!currentChatId) {
        const res = await fetch("http://localhost:8000/chats", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ title: file.name }),
        });
        if (!res.ok) throw new Error("Failed to create chat");
        const newChat = await res.json();
        setChats((prev) => [newChat, ...prev]);
        setActiveChatId(newChat.id);
        currentChatId = newChat.id;
      }

      const formData = new FormData();
      formData.append("file", file);
      formData.append("chatId", currentChatId);

      const res = await fetch("http://localhost:8000/documents", {
        method: "POST",
        body: formData,
      });
      if (!res.ok) throw new Error("Upload failed");
      
      setUploadedFile({ name: file.name });
    } catch (err) {
      console.error(err);
      alert("Failed to upload document.");
    } finally {
      setUploading(false);
    }
  }

  return (
    <div className="app">
      <Sidebar
        open={sidebarOpen}
        chats={chats}
        notebooks={notebooks}
        activeChatId={activeChatId}
        screen={screen}
        onToggle={() => setSidebarOpen((open) => !open)}
        onNewChat={startNewChat}
        onHistory={openHistory}
        onOpenChat={openChat}
        onAddNotebook={addNotebook}
      />
      <main className="main">
        {screen === "history" && chats.length > 0 ? (
          <HistoryScreen
            chats={history}
            query={historyQuery}
            onQuery={setHistoryQuery}
            onOpen={openChat}
          />
        ) : (
          <ChatScreen
            chat={activeChat}
            hasHistory={chats.length > 0}
            draft={draft}
            pending={pending}
            model={model}
            uploadedFile={uploadedFile}
            uploading={uploading}
            onDraft={setDraft}
            onModel={setModel}
            onSend={sendMessage}
            onUpload={handleFileUpload}
            onClearFile={() => setUploadedFile(null)}
          />
        )}
      </main>
    </div>
  );
}

function Sidebar({
  open,
  chats,
  notebooks,
  activeChatId,
  screen,
  onToggle,
  onNewChat,
  onHistory,
  onOpenChat,
  onAddNotebook,
}) {
  return (
    <aside className={open ? "sidebar" : "sidebar sidebar-collapsed"}>
      <div className="sidebar-top">
        <div className="brand">
          <SparkMark />
          {open && <span>Chat</span>}
        </div>
        <button className="icon-button" type="button" onClick={onToggle} aria-label="Collapse sidebar">
          <PanelIcon />
        </button>
      </div>

      {open && (
        <>
          <nav className="nav">
            <button type="button" onClick={onNewChat}>
              <ComposeIcon /> New chat
            </button>
            <button type="button">
              <LibraryIcon /> Library
            </button>
          </nav>

          <section className="side-section">
            <p>Notebooks</p>
            <button type="button" onClick={onAddNotebook}>
              <PlusIcon /> New notebook
            </button>
            {notebooks.map((notebook) => (
              <button type="button" key={notebook.id}>
                <NotebookIcon /> {notebook.title}
              </button>
            ))}
          </section>

          {chats.length > 0 && (
            <section className="side-section recent">
              <button type="button" className="history-label" onClick={onHistory}>
                History
              </button>
              {chats.map((chat) => (
                <button
                  type="button"
                  key={chat.id}
                  className={screen === "chat" && chat.id === activeChatId ? "selected" : ""}
                  onClick={() => onOpenChat(chat.id)}
                >
                  {chat.title}
                </button>
              ))}
            </section>
          )}

          <div className="profile">
            <span className="avatar">N</span>
            <span>
              <strong>Nikhil Chauhan</strong>
              <small>Pro</small>
            </span>
            <button className="icon-button" type="button" aria-label="Settings">
              <GearIcon />
            </button>
          </div>
        </>
      )}
    </aside>
  );
}

function ChatScreen({ chat, hasHistory, draft, pending, model, uploadedFile, uploading, onDraft, onModel, onSend, onUpload, onClearFile }) {
  const messages = chat?.messages ?? [];

  return (
    <section className="chat-screen">
      <div className="thread">
        {messages.length === 0 && (
          <div className="welcome">
            <h1>Welcome</h1>
            <p>{hasHistory ? "Start a new chat, or open one from History." : "No chats yet. Ask a question to begin."}</p>
          </div>
        )}
        {messages.map((message) => (
          <article key={message.id || message.role + message.content} className={message.role === "user" ? "message user" : "message assistant"}>
            {message.role === "assistant" ? <Answer text={message.content} /> : <p>{message.content}</p>}
          </article>
        ))}
        {pending && <p className="pending">Writing the full answer…</p>}
      </div>
      <Composer 
        draft={draft} 
        model={model} 
        pending={pending} 
        uploadedFile={uploadedFile} 
        uploading={uploading} 
        onDraft={onDraft} 
        onModel={onModel} 
        onSend={onSend} 
        onUpload={onUpload} 
        onClearFile={onClearFile} 
      />
      <p className="disclaimer">Answers are shown in full when the response is ready.</p>
    </section>
  );
}

function HistoryScreen({ chats, query, onQuery, onOpen }) {
  return (
    <section className="history-screen">
      <header>
        <h1>History</h1>
      </header>
      <label className="history-search">
        <SearchIcon />
        <input
          value={query}
          onChange={(event) => onQuery(event.target.value)}
          placeholder="Search chats"
        />
      </label>
      <ul>
        {chats.length === 0 && (
          <li className="history-empty">No chats match that search.</li>
        )}
        {chats.map((chat) => {
          const preview = chat.messages[0]?.content ?? "";
          return (
            <li key={chat.id}>
              <button type="button" onClick={() => onOpen(chat.id)}>
                <span>
                  <strong>{chat.title}</strong>
                  <small>{preview}</small>
                </span>
                <em>{chat.updatedAt}</em>
              </button>
            </li>
          );
        })}
      </ul>
    </section>
  );
}

function Answer({ text }) {
  const blocks = (text || "").split("\n");

  // Helper to parse **bold** text
  const parseFormatting = (line) => {
    const parts = line.split(/(\*\*.*?\*\*)/g);
    return parts.map((part, i) => {
      if (part.startsWith("**") && part.endsWith("**")) {
        return <strong key={i}>{part.slice(2, -2)}</strong>;
      }
      return part;
    });
  };

  return (
    <div className="answer">
      {blocks.map((line, index) => {
        const trimmed = line.trim();
        if (!trimmed) return <div key={index} className="spacer" />;
        if (/^\d+\.\s/.test(trimmed)) return <h2 key={index}>{parseFormatting(trimmed)}</h2>;
        if (trimmed.startsWith("•") || trimmed.startsWith("-")) {
          // Remove the dash or bullet from the start
          const textContent = trimmed.replace(/^[•-]\s*/, "");
          return <p key={index} className="bullet">• {parseFormatting(textContent)}</p>;
        }
        if (trimmed.startsWith("#")) {
          return <h2 key={index}>{parseFormatting(trimmed.replace(/^#+\s*/, ""))}</h2>;
        }
        return <p key={index}>{parseFormatting(trimmed)}</p>;
      })}
    </div>
  );
}

function Composer({ draft, model, pending, uploadedFile, uploading, onDraft, onModel, onSend, onUpload, onClearFile }) {
  return (
    <div className="composer-container">
      {(uploadedFile || uploading) && (
        <div className="composer-attachment">
          <span className="attachment-name">
            {uploading ? "Uploading..." : uploadedFile.name}
          </span>
          {!uploading && (
            <button type="button" className="attachment-clear" onClick={onClearFile} aria-label="Remove attachment">
              ✕
            </button>
          )}
        </div>
      )}
      <form
        className="composer"
        onSubmit={(event) => {
          event.preventDefault();
          onSend();
        }}
      >
        <label className="plus" aria-label="Add Document" style={{ cursor: "pointer", display: "flex", alignItems: "center", justifyContent: "center" }}>
          <PlusIcon />
          <input type="file" accept=".txt,.md,.pdf" style={{ display: "none" }} onChange={(e) => {
             onUpload(e);
             e.target.value = null; 
          }} />
        </label>
        <textarea
          value={draft}
          placeholder="Ask anything"
          rows={1}
          style={{ resize: "none", fontFamily: "inherit", fontSize: "inherit", overflowY: "auto", maxHeight: "150px" }}
          onChange={(event) => {
            onDraft(event.target.value);
            // Simple auto-resize logic
            event.target.style.height = 'auto';
            event.target.style.height = Math.min(event.target.scrollHeight, 150) + 'px';
          }}
          onKeyDown={(event) => {
            if (event.key === 'Enter' && !event.shiftKey) {
              event.preventDefault();
              if (draft.trim() && !pending) {
                onSend();
                // Reset height
                event.target.style.height = 'auto';
              }
            }
          }}
        />
        <label className="model-picker">
          <span className="sr-only">Model</span>
          <select value={model} onChange={(event) => onModel(event.target.value)}>
            <option>Pro</option>
            <option>Fast</option>
          </select>
        </label>
        <button className="mic" type="button" aria-label="Microphone">
          <MicIcon />
        </button>
        <button className="send" type="submit" disabled={pending || !draft.trim()} aria-label="Send">
          Send
        </button>
      </form>
    </div>
  );
}

function SparkMark() {
  return (
    <svg className="mark" viewBox="0 0 32 32" aria-hidden="true">
      <path fill="#4285F4" d="M16 2l2.2 8.2L26 12l-7.8 1.8L16 22l-2.2-8.2L6 12l7.8-1.8z" />
      <path fill="#EA4335" d="M24 16l1.2 4 4 1.2-4 1.2-1.2 4-1.2-4-4-1.2 4-1.2z" />
      <path fill="#FBBC04" d="M8 18l.8 2.6 2.7.8-2.7.8L8 25l-.8-2.8L4.5 21.4l2.7-.8z" />
      <circle cx="23" cy="8" r="2" fill="#34A853" />
    </svg>
  );
}

function PanelIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <rect x="3" y="4" width="18" height="16" rx="2" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <path d="M9 4v16" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function ComposeIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M4 17.5V20h2.5L17 9.5 14.5 7 4 17.5z" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <path d="M13 8.5l2.5 2.5" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function SearchIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="11" cy="11" r="6" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <path d="M16 16l4 4" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function LibraryIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <rect x="4" y="4" width="6" height="6" rx="1" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <rect x="14" y="4" width="6" height="6" rx="1" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <rect x="4" y="14" width="6" height="6" rx="1" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <rect x="14" y="14" width="6" height="6" rx="1" fill="none" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function PlusIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  );
}

function NotebookIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <rect x="6" y="3" width="12" height="18" rx="2" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <path d="M9 8h6M9 12h6" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function GearIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="12" cy="12" r="3" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <path d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M18.4 5.6 17 7M7 17l-1.4 1.4" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function MicIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <rect x="9" y="3" width="6" height="11" rx="3" fill="none" stroke="currentColor" strokeWidth="1.6" />
      <path d="M6 11a6 6 0 0 0 12 0M12 17v4" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}
