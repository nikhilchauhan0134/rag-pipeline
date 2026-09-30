# Document Analysis RAG Pipeline

A full-stack Retrieval-Augmented Generation (RAG) application specifically designed for **Document Analysis**. Users can upload documents (like PDFs or text files), and the AI will securely answer questions strictly based on the provided document context, completely eliminating outside hallucinations.

## ✨ Key Features

- **Isolated Vector Searches (Chat Scoping):** Each chat session is completely isolated. Uploaded documents are tagged and scoped strictly to the current chat, preventing the AI from accidentally mixing up context from older uploaded PDFs.
- **Native PDF Extraction:** Automatically parses binary PDF files, extracts the text, and processes it using overlapping chunking strategies (e.g., 2000 chars with 200 overlap) to efficiently fit within LLM context windows.
- **Dynamic Chat Titles:** The application automatically renames your chat in the history sidebar to match the name of the file you uploaded or the first question you asked.
- **Auto-Resizing Chat UI:** The chat input box expands vertically as you type or paste large prompts, automatically introducing a scrollbar when it gets too long.
- **Fully Automated CI/CD:** Integrated GitHub Actions pipeline that automatically builds Docker images, pushes them to GitHub Container Registry, and pings Render Deploy Hooks for instant cloud deployment.

## 🛠 Tech Stack

- **Frontend:** React, Vite, JavaScript
- **Backend:** .NET 10 Web API, C#
- **AI Integration:** Groq API (High-speed LLM inference)
- **Vector Database:** Qdrant Cloud (with an InMemory fallback)
- **Infrastructure:** Docker, Docker Compose, GitHub Actions, Render

---

## 🚀 Local Setup Instructions

### Prerequisites
1. Ensure you have [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and currently running on your machine.
2. Get a free API Key from [Groq](https://console.groq.com/).

### Running the Project

1. **Clone the repository:**
   ```bash
   git clone https://github.com/nikhilchauhan0134/rag-pipeline.git
   cd rag-pipeline
   ```

2. **Set your Groq API Key:**
   You must set this environment variable so the backend can communicate with the AI model.
   - **Windows (PowerShell):**
     ```powershell
     $env:GROQ_API_KEY="your_groq_api_key_here"
     ```
   - **Mac/Linux (Terminal):**
     ```bash
     export GROQ_API_KEY="your_groq_api_key_here"
     ```

3. **Build and start the containers:**
   ```bash
   docker compose up --build -d
   ```

4. **Access the application:**
   - **Web UI:** http://localhost:3000
   - **Backend API:** http://localhost:8000

---

## ☁️ Production Deployment (Render)

This project is fully wired for continuous deployment to [Render](https://render.com). 

Whenever you push to the `main` branch, GitHub Actions will automatically:
1. Run Unit Tests.
2. Build the Docker images for both the API and UI.
3. Push the images to `ghcr.io`.
4. Trigger the Render Deploy Hooks to update the live servers.

**To complete the cloud setup:**
1. In your Render Dashboard, ensure you have two Web Services pulling from `ghcr.io/nikhilchauhan0134/ragpipeline-api:latest` and `ghcr.io/nikhilchauhan0134/ragpipeline-ui:latest`.
2. Add the `GROQ_API_KEY` to the Environment Variables of your **API Web Service** in Render.
3. Add your Render Deploy Hook URLs as **GitHub Secrets** (`RENDER_API_DEPLOY_HOOK` and `RENDER_UI_DEPLOY_HOOK`) in your repository settings to enable the automated CI/CD pipeline.
