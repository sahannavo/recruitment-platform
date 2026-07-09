export interface WorkExperience {
  id: string;
  role: string;
  company: string;
  duration: string;
  description: string;
  logoType?: 'apartment' | 'business' | 'corporate';
}

export interface Education {
  id: string;
  degree: string;
  school: string;
  duration: string;
}

export interface CandidateProfile {
  fullName: string;
  email: string;
  phone: string;
  location: string;
  linkedinUrl: string;
  avatarUrl: string;
  roleTitle: string;
  experience: WorkExperience[];
  education: Education[];
  resumeName?: string;
  resumeParsed?: boolean;
}

export interface ConnectedServices {
  googleCalendar: {
    connected: boolean;
  };
  sendGrid: {
    connected: boolean;
    apiKey: string;
  };
}

export interface EmailTemplate {
  templateType: string;
  subject: string;
  body: string;
}

export interface AdminSettingsState {
  companyName: string;
  timezone: string;
  logoUrl?: string;
  skillWeight: number;
  experienceWeight: number;
  culturalWeight: number;
  services: ConnectedServices;
  emailTemplates: EmailTemplate[];
}

export type AppPortal = 'candidate' | 'recruiter_analytics' | 'recruiter_settings';
