# AI-Powered Recruitment Platform

A comprehensive, AI-enhanced recruitment platform that connects candidates with recruiters. Built with a modern C# backend and a sleek, vanilla HTML/CSS/JS frontend.

## Features

### For Candidates
* **Profile Management**: Manage your skills, experience, and upload your CV.
* **AI Bio Generation**: Automatically generate a professional biography using AI based on your profile details.
* **Job Search**: Browse and apply for open job positions.
* **Application Tracking**: Track the status of your applications and withdraw them if needed.

### For Recruiters / Hiring Managers
* **Job Management**: Post new jobs, manage existing listings, and track candidate applications.
* **AI Candidate Matching**: The system automatically scores and ranks candidates against job requirements using AI.
* **Interview Scheduling**: Easily schedule interviews (Online or In-Person). Add meeting links, Zoom IDs, or physical location instructions.
* **Automated Emails**: Automatically send interview invitations and updates to candidates via email.

### For Administrators
* **Built-in Admin Account**: Out-of-the-box admin credentials for quick setup.
* **Dynamic Settings UI**: Manage platform settings directly from the frontend, including:
  * Company details and branding
  * AI configurations (creativity, precision, penalty)
  * API Keys (OpenRouter/OpenAI, SendGrid, AWS)

## Technology Stack

* **Backend**: C# 12, ASP.NET Core 8 Web API
* **Database**: Entity Framework Core (SQL Server / LocalDB)
* **Frontend**: Vanilla HTML5, CSS3, JavaScript (ES6)
* **Integrations**:
  * **OpenRouter / OpenAI**: Powers AI features like resume parsing, bio generation, and candidate matching.
  * **SendGrid**: Transactional emails and interview invitations.

## Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* SQL Server Express LocalDB (or any SQL Server instance)

### Backend Setup
1. Navigate to the backend directory:
   ```bash
   cd backend/RecruitmentAPI
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
   *(Note: The application will **automatically apply database migrations** and seed the database with default roles, an admin account, and default platform settings upon startup. You do not need to run `dotnet ef database update` manually unless you are making schema changes).*
   
   The API and the static frontend will be available at `https://localhost:5001` or `http://localhost:5000`.

### Accessing the Platform
Since the ASP.NET Core backend automatically serves the frontend static files, you don't need a separate web server.
1. Simply run the backend (`dotnet run`).
2. Open your browser and navigate to `https://localhost:5001/auth/login.html` (or `http://localhost:5000/auth/login.html`).

### Default Credentials
Upon first run, the database is seeded with a default Administrator account:
* **Email**: `admin@recruitai.com`
* **Password**: `Admin@123`

Once logged in as an admin, navigate to the **Settings** tab to configure your AI (OpenRouter) and Email (SendGrid) API keys.

## Recent Updates
* Added dynamic AI and Email API key management via the Admin Settings UI.
* Fixed strict URL validation on the interview scheduling page to allow both meeting links and custom connection instructions.
* Removed automatic AI CV parsing during upload per user request (fallback to manual entry with AI bio generation).
* Auto-seeding default Admin account on database initialization.

  Contributors
  P D S Sandawaruni-https://github.com/Subodha2004
