# RecruitAI Platform
A modern, AI-powered recruitment platform designed to streamline the hiring process for Candidates, Recruiters, Hiring Managers, and Administrators.
## 🚀 Features
- **For Candidates:** 
  - Upload CVs (PDF/TXT) with automatic AI parsing for skills.
  - Generate a professional AI-written biography based on your profile.
  - Apply to job postings and view your AI match score against job requirements.
  - Track application status and interview schedules.
- **For Recruiters:**
  - Create and manage Job Postings.
  - Review applications and see AI-calculated match percentages for each candidate.
  - Schedule interviews and automatically send email notifications to candidates.
- **For Hiring Managers:**
  - View dashboard analytics of current open roles.
  - Review shortlisted candidates directly from the dashboard.
  - View upcoming interviews.
- **For Administrators:**
  - Manage system settings and platform configuration.
  - View global recruitment analytics and reports.
  - Manage users across the platform.
## 🧠 Key AI Capabilities
This platform leverages cutting-edge Artificial Intelligence to modernize recruitment:
- **Intelligent Resume Parsing:** Candidates can upload their CVs (PDF/TXT), and the AI automatically reads the file and extracts their core skills into the database.
- **AI Biography Generation:** Based on a candidate's uploaded skills and existing summary, the AI crafts a polished, professional biography with bolded sections (like Background, Key Strengths, and Next Steps) that the candidate can copy into their profile.
- **Automated Candidate-Job Matching:** The system dynamically computes an AI-driven "Match Score" by comparing a candidate's saved skills against the required skills for a job posting, allowing recruiters to instantly see the most qualified applicants.
- **Smart Analytics:** Empowers hiring managers with data-driven insights into the hiring funnel.
## 🛠️ Technology Stack
- **Frontend:** Vanilla HTML5, CSS3, JavaScript (No heavy frameworks, clean and fast).
- **Backend:** C# / ASP.NET Core Web API
- **Database:** SQLite (via Entity Framework Core)
- **External Services:**
  - **OpenRouter AI:** Used for AI resume parsing and biography generation.
  - **SendGrid:** Used for sending interview invitations and email notifications.
  - **Azure Blob Storage:** Used for storing candidate CV uploads securely.
---
## ⚙️ How to Clone & Run Locally
### Prerequisites
1. Ensure you have the **[.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)** installed.
2. Ensure you have an IDE like **Visual Studio Code** or **Visual Studio**.
3. (Optional) Install the "Live Server" extension in VS Code to easily serve the frontend files.
### 1. Clone the Repository
```bash
git clone https://github.com/sahannavo/recruitment-platform.git
cd recruitment-platform
```
### 2. Configure Environment Variables & Secrets
The platform relies on a few external APIs to function fully. You must provide your own API keys.
1. Navigate to the backend directory:
   ```bash
   cd backend/RecruitmentAPI
   ```
2. Make a copy of the template environment file:
   - Copy `.env.example` and rename the new file to `.env`
3. Open `.env` and fill in your actual API keys:
   ```ini
   Notifications__SendGridApiKey=YOUR_SENDGRID_KEY
   AIService__ApiKey=YOUR_OPENROUTER_KEY
   ConnectionStrings__AzureBlobStorage=DefaultEndpointsProtocol=https;AccountName=...
   ```
*(Note: Do not commit your real `.env` file to GitHub! It is already ignored in the `.gitignore`.)*
### 3. Run the Backend (API Server)
While still inside the `backend/RecruitmentAPI` folder, run the following commands:
```bash
# Restore dependencies
dotnet restore
# Run the application
dotnet run
```
The backend server will start running on `http://localhost:5000` (and `https://localhost:5001`). 
*The SQLite database (`app.db`) will automatically be created and migrated upon starting.*
**✨ Magic Feature:** The moment the backend server finishes starting up, it will automatically launch your default browser and open the `frontend/index.html` file for you!

### 4. Run the Frontend (Optional / For Live Reloading)
Because the backend automatically opens the frontend for you, you don't *strictly* need a server for the frontend. However, if you are actively editing the HTML/CSS and want **Live Reloading**, you can use VS Code:
1. Open the project folder in **Visual Studio Code**.
2. Install the **Live Server** extension (if you haven't already).
3. Right-click on `frontend/index.html` and select **"Open with Live Server"**.
4. Your browser will open the application (typically at `http://127.0.0.1:5500/frontend/index.html`).
5. Click on **Sign Up** to create a new Candidate account, or **Log In** if you already have accounts set up!
## 🧪 Testing the Platform
- **Authentication:** You can register a new account on the login page. New registrations default to the `Candidate` role.
- **Mock Data/Roles:** If you need to test the Recruiter, Hiring Manager, or Admin views, you can manually update the `Role` column for a user inside the local SQLite database (`backend/RecruitmentAPI/app.db`) using a tool like DB Browser for SQLite.
## 🤝 Contributing
1. Create a feature branch (`git checkout -b feature/my-feature`)
2. Commit your changes (`git commit -m 'Add some feature'`)
3. Push to the branch (`git push origin feature/my-feature`)
4. Open a Pull Request
## 📝 License
This project is proprietary and confidential.
