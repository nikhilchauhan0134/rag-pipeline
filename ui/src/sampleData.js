export const initialChats = [
  {
    id: "vector-db",
    title: "Free Vector Databases for Demos",
    updatedAt: "Today",
    messages: [
      {
        id: "vector-db-q",
        role: "user",
        content: "Which free vector databases are fine for a demo?",
      },
      {
        id: "vector-db-a",
        role: "assistant",
        content:
          "A free local vector database is enough for a demo.\n\nChroma and Qdrant both run on your machine without a paid account. Store embeddings there, then send the retrieved passages into the prompt.",
      },
    ],
  },
  {
    id: "rag-net",
    title: "Building RAG Pipelines in .NET",
    updatedAt: "Today",
    messages: [
      {
        id: "rag-net-q",
        role: "user",
        content: "How does a RAG pipeline fit in a .NET API?",
      },
      {
        id: "rag-net-a",
        role: "assistant",
        content:
          "The API receives the question, searches the vector database, and waits for the full answer before it responds.\n\nThat call stays synchronous: one request in, one complete answer out.",
      },
    ],
  },
  {
    id: "agents",
    title: "Understanding LLM Agents and Tool Use",
    updatedAt: "Yesterday",
    messages: [
      {
        id: "agents-q",
        role: "user",
        content: "Explain tools, memory, and planning for an LLM agent.",
      },
      {
        id: "agents-a",
        role: "assistant",
        content: `2. Tools (Action Space): These are the external functions the LLM is given permission to trigger. A tool is usually just a RESTful API or a local function. Examples include:

• A web search tool.
• A code execution environment (e.g., running a C# script to test logic).
• A SQL Server connection to fetch live data.
• A calculator for precise math.

3. Memory:

• Short-term memory: Keeping track of the current thought process and steps already taken.
• Long-term memory: Often implemented using Retrieval-Augmented Generation (RAG) and vector databases, allowing the agent to recall past interactions or reference vast amounts of company documentation.

4. Planning (Orchestration): Frameworks that guide how the agent thinks. A common approach is ReAct (Reason + Act), where the agent enters a loop:

• Thought: "I need to find the user's details before I can reset their password."`,
      },
    ],
  },
  {
    id: "port",
    title: "Resolving Port 63251 Conflict",
    updatedAt: "Yesterday",
    messages: [
      {
        id: "port-q",
        role: "user",
        content: "What should I check when port 63251 is already in use?",
      },
      {
        id: "port-a",
        role: "assistant",
        content:
          "Find the process listening on that port, stop it, or run the app on another port.\n\nOn Windows, netstat can show which process owns 63251.",
      },
    ],
  },
  {
    id: "csharp",
    title: "C# Object Initializers Guide",
    updatedAt: "Last week",
    messages: [
      {
        id: "csharp-q",
        role: "user",
        content: "Show a short example of a C# object initializer.",
      },
      {
        id: "csharp-a",
        role: "assistant",
        content:
          "An object initializer sets properties as the instance is created.\n\nvar chat = new Chat { Title = \"New chat\", UpdatedAt = \"Today\" };",
      },
    ],
  },
];

export const initialNotebooks = [
  { id: "notebook-1", title: "Untitled notebook" },
];
