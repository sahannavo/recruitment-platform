/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import { useState } from "react";
import Header from "./components/Header";
import CandidateProfile from "./components/CandidateProfile";
import AnalyticsDashboard from "./components/AnalyticsDashboard";
import AdminSettings from "./components/AdminSettings";
import { AppPortal, CandidateProfile as ProfileType, AdminSettingsState } from "./types";

// Default Profile Matching Screenshot 1
const initialProfile: ProfileType = {
  fullName: "Alex Rivers",
  roleTitle: "Senior UI Designer",
  location: "San Francisco, CA",
  email: "alex.rivers@design.com",
  phone: "+1 (555) 019-2834",
  linkedinUrl: "linkedin.com/in/alexrivers",
  avatarUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuAPcXjPinq21DzokgVaxRAV-T9EUfxbr5qiERIVihvgrpoEXi15qJmhl590x3s1P3InmEtIbMYiMG0unb-pMruZtTzhbDugGXpN_-usPYcmwFZ1lkmHTqWvH568X96Oq90BR32so7EwvxvCi8jyJSJn4dg73b357B-DweaRp2XR56Dy4PhwhU5XOl_sSDLllM2omHiqXR5CXDXU6kzSbmFIa04kZLRAQTYBkIzqME-bD4VLu1ckh4RWLI4haNYq8blg6VOwbZjA2Bw7",
  experience: [
    {
      id: "exp-1",
      role: "Senior UI Designer",
      company: "TechFlow Inc.",
      duration: "Jan 2021 - Present • 3 yrs 5 mos",
      description: "Lead the redesign of the core SaaS platform, improving user retention by 24%. Managed a team of 3 junior designers and established the company's first comprehensive design system."
    }
  ],
  education: [
    {
      id: "edu-1",
      degree: "BFA in Interaction Design",
      school: "California College of the Arts",
      duration: "2015 - 2019"
    }
  ]
};

// Default Settings Matching Screenshot 3
const initialSettings: AdminSettingsState = {
  companyName: "Kinetic Talent Partners",
  timezone: "Coordinated Universal Time (UTC)",
  skillWeight: 70,
  experienceWeight: 50,
  culturalWeight: 30,
  services: {
    googleCalendar: { connected: true },
    sendGrid: { connected: false, apiKey: "" }
  },
  emailTemplates: [
    {
      templateType: "Interview Invite",
      subject: "Invitation to Interview: {Job_Title} at {Company_Name}",
      body: "Hi {Candidate_First_Name},\n\nThank you for applying for the {Job_Title} position. We were impressed by your background and would like to invite you to an initial screening call.\n\nPlease use the link below to select a time that works best for you:\n{Scheduling_Link}\n\nWe look forward to speaking with you!\n\nBest regards,\nThe {Company_Name} Team"
    },
    {
      templateType: "Application Received",
      subject: "We've received your application for {Job_Title}!",
      body: "Hi {Candidate_First_Name},\n\nThank you for submitting your resume. Our team is actively reviewing submissions and will be in touch shortly.\n\nBest,\nThe RecruitAI Team"
    },
    {
      templateType: "Rejection",
      subject: "Update regarding {Job_Title} at {Company_Name}",
      body: "Hi {Candidate_First_Name},\n\nThank you for taking the time to meet with us. While we were impressed with your credentials, we have decided to move forward with other candidates at this time.\n\nWe wish you the best of luck in your search.\n\nRegards,\nThe RecruitAI Team"
    },
    {
      templateType: "Offer Extended",
      subject: "Offer of Employment: {Job_Title} at {Company_Name}!",
      body: "Dear {Candidate_First_Name},\n\nWe are absolutely thrilled to extend you a formal offer of employment to join us as a {Job_Title}.\n\nPlease review the attached document and let us know your decision by Friday.\n\nBest,\nThe {Company_Name} Team"
    }
  ]
};

export default function App() {
  const [currentPortal, setCurrentPortal] = useState<AppPortal>("candidate");
  const [profile, setProfile] = useState<ProfileType>(initialProfile);
  const [settings, setSettings] = useState<AdminSettingsState>(initialSettings);

  const handlePortalChange = (portal: AppPortal) => {
    setCurrentPortal(portal);
  };

  const handleProfileUpdate = (updatedProfile: ProfileType) => {
    setProfile(updatedProfile);
  };

  const handleSettingsUpdate = (updatedSettings: AdminSettingsState) => {
    setSettings(updatedSettings);
  };

  return (
    <div className="bg-slate-50 min-h-screen flex flex-col font-sans selection:bg-indigo-100 selection:text-indigo-900 antialiased">
      {/* Recruiter & Settings views share a top Recruiter header */}
      {currentPortal !== "candidate" && (
        <Header
          currentPortal={currentPortal}
          onPortalChange={handlePortalChange}
          companyName={settings.companyName}
        />
      )}

      {/* Render selected Portal */}
      <div className="flex-1">
        {currentPortal === "candidate" && (
          <CandidateProfile
            profile={profile}
            onProfileUpdate={handleProfileUpdate}
            onViewAnalytics={() => setCurrentPortal("recruiter_analytics")}
          />
        )}

        {currentPortal === "recruiter_analytics" && (
          <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <AnalyticsDashboard settings={settings} />
          </main>
        )}

        {currentPortal === "recruiter_settings" && (
          <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <AdminSettings
              settings={settings}
              onSettingsUpdate={handleSettingsUpdate}
            />
          </main>
        )}
      </div>
    </div>
  );
}

