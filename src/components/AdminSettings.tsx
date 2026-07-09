import React, { useState } from "react";
import {
  Building,
  Upload,
  Sliders,
  Calendar,
  Mail,
  FileText,
  CheckCircle,
  AlertCircle,
  Key,
  Database,
  Globe,
  Bell,
  Clock,
  Sparkles
} from "lucide-react";
import { AdminSettingsState, EmailTemplate } from "../types";

interface AdminSettingsProps {
  settings: AdminSettingsState;
  onSettingsUpdate: (updated: AdminSettingsState) => void;
}

type SettingsTab = "general" | "ai_weights" | "integrations" | "notifications";

export default function AdminSettings({ settings, onSettingsUpdate }: AdminSettingsProps) {
  const [activeTab, setActiveTab] = useState<SettingsTab>("general");

  // Local form states to preserve edits before saving
  const [companyName, setCompanyName] = useState(settings.companyName);
  const [timezone, setTimezone] = useState(settings.timezone);
  const [logoName, setLogoName] = useState<string | null>(null);

  // AI Weight states
  const [skillWeight, setSkillWeight] = useState(settings.skillWeight);
  const [experienceWeight, setExperienceWeight] = useState(settings.experienceWeight);
  const [culturalWeight, setCulturalWeight] = useState(settings.culturalWeight);

  // Integrations states
  const [calendarConnected, setCalendarConnected] = useState(settings.services.googleCalendar.connected);
  const [sendGridConnected, setSendGridConnected] = useState(settings.services.sendGrid.connected);
  const [sendGridKey, setSendGridKey] = useState(settings.services.sendGrid.apiKey);

  // Notifications states
  const [selectedTemplateType, setSelectedTemplateType] = useState("Interview Invite");
  const [templates, setTemplates] = useState<EmailTemplate[]>(settings.emailTemplates);

  // Current active template form state
  const activeTemplate = templates.find((t) => t.templateType === selectedTemplateType) || templates[0];
  const [subjectInput, setSubjectInput] = useState(activeTemplate.subject);
  const [bodyInput, setBodyInput] = useState(activeTemplate.body);

  // Toast status feedback
  const [saveToast, setSaveToast] = useState<string | null>(null);

  // Track template selection change and update subject/body states
  const handleTemplateTypeChange = (type: string) => {
    // First save the current edited template to the local list
    const updatedList = templates.map((t) => {
      if (t.templateType === selectedTemplateType) {
        return { ...t, subject: subjectInput, body: bodyInput };
      }
      return t;
    });
    setTemplates(updatedList);

    // Switch to new selected template
    setSelectedTemplateType(type);
    const nextTemplate = updatedList.find((t) => t.templateType === type);
    if (nextTemplate) {
      setSubjectInput(nextTemplate.subject);
      setBodyInput(nextTemplate.body);
    }
  };

  const handleGlobalSave = () => {
    // Sync current active template
    const finalTemplates = templates.map((t) => {
      if (t.templateType === selectedTemplateType) {
        return { ...t, subject: subjectInput, body: bodyInput };
      }
      return t;
    });

    onSettingsUpdate({
      companyName,
      timezone,
      logoUrl: logoName || settings.logoUrl,
      skillWeight,
      experienceWeight,
      culturalWeight,
      services: {
        googleCalendar: { connected: calendarConnected },
        sendGrid: { connected: sendGridConnected, apiKey: sendGridKey }
      },
      emailTemplates: finalTemplates
    });

    setSaveToast("Settings changes saved successfully!");
    setTimeout(() => setSaveToast(null), 3000);
  };

  const triggerTestEmail = () => {
    setSaveToast(`Mock test email sent successfully using template: "${selectedTemplateType}"!`);
    setTimeout(() => setSaveToast(null), 3500);
  };

  return (
    <div className="space-y-6">
      {/* Settings Title */}
      <div>
        <h1 className="text-3xl font-bold font-sans text-slate-900 tracking-tight">Settings</h1>
        <p className="text-sm text-slate-500 mt-1">Manage system configurations, AI behaviors, and third-party integrations.</p>
      </div>

      {/* Save Toast Feedback */}
      {saveToast && (
        <div className="bg-indigo-50 border border-indigo-200 rounded-xl p-4 flex items-start gap-3 shadow-xs animate-in slide-in-from-top-4 duration-300">
          <CheckCircle className="w-5 h-5 text-indigo-600 shrink-0 mt-0.5" />
          <p className="text-sm font-semibold text-indigo-900">{saveToast}</p>
        </div>
      )}

      {/* Settings Navigation Tabs */}
      <div className="border-b border-slate-200">
        <nav className="flex space-x-8 overflow-x-auto scrollbar-none" aria-label="Tabs">
          <button
            onClick={() => setActiveTab("general")}
            className={`pb-4 px-1 border-b-2 font-medium text-sm whitespace-nowrap transition-all cursor-pointer ${
              activeTab === "general"
                ? "border-indigo-600 text-indigo-600 font-bold"
                : "border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300"
            }`}
          >
            General
          </button>
          <button
            onClick={() => setActiveTab("ai_weights")}
            className={`pb-4 px-1 border-b-2 font-medium text-sm whitespace-nowrap transition-all cursor-pointer ${
              activeTab === "ai_weights"
                ? "border-indigo-600 text-indigo-600 font-bold"
                : "border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300"
            }`}
          >
            AI Weights
          </button>
          <button
            onClick={() => setActiveTab("integrations")}
            className={`pb-4 px-1 border-b-2 font-medium text-sm whitespace-nowrap transition-all cursor-pointer ${
              activeTab === "integrations"
                ? "border-indigo-600 text-indigo-600 font-bold"
                : "border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300"
            }`}
          >
            Integrations
          </button>
          <button
            onClick={() => setActiveTab("notifications")}
            className={`pb-4 px-1 border-b-2 font-medium text-sm whitespace-nowrap transition-all cursor-pointer ${
              activeTab === "notifications"
                ? "border-indigo-600 text-indigo-600 font-bold"
                : "border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300"
            }`}
          >
            Notifications
          </button>
        </nav>
      </div>

      {/* Main Settings Panel Wrapper */}
      <div className="bg-white border border-slate-200 rounded-xl shadow-xs p-6 md:p-8">

        {/* 1. General Settings Tab */}
        {activeTab === "general" && (
          <div className="space-y-6 animate-in fade-in duration-200">
            <div>
              <h3 className="font-bold text-slate-900 text-base flex items-center gap-2">
                <Building className="w-5 h-5 text-indigo-600" />
                Company Profile
              </h3>
              <p className="text-xs text-slate-400 mt-1">Configure company-specific metadata for the recruiter interface.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pt-2">
              {/* Left Column: Form Inputs */}
              <div className="space-y-4">
                <div className="space-y-1">
                  <label className="text-xs font-semibold text-slate-500 ml-1">Company Name</label>
                  <input
                    type="text"
                    value={companyName}
                    onChange={(e) => setCompanyName(e.target.value)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all"
                    placeholder="Kinetic Talent Partners"
                  />
                  <p className="text-[10px] text-slate-400 italic mt-1">
                    *Changing this instantly updates the top-left logo in your recruiter header!
                  </p>
                </div>

                <div className="space-y-1">
                  <label className="text-xs font-semibold text-slate-500 ml-1">Primary Timezone</label>
                  <div className="relative">
                    <Clock className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 w-4 h-4" />
                    <select
                      value={timezone}
                      onChange={(e) => setTimezone(e.target.value)}
                      className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-10 pr-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all appearance-none cursor-pointer"
                    >
                      <option>Pacific Time (PT) - US & Canada</option>
                      <option>Eastern Time (ET) - US & Canada</option>
                      <option>Coordinated Universal Time (UTC)</option>
                      <option>Central European Time (CET)</option>
                    </select>
                  </div>
                </div>
              </div>

              {/* Right Column: Logo Upload */}
              <div className="space-y-1">
                <label className="text-xs font-semibold text-slate-500 ml-1">Company Logo</label>
                <div className="mt-1 flex flex-col items-center justify-center border-2 border-dashed border-slate-200 rounded-xl px-4 py-8 bg-slate-50 hover:bg-indigo-50/10 hover:border-indigo-500 transition-colors cursor-pointer group">
                  <Upload className="w-8 h-8 text-slate-400 group-hover:text-indigo-600 transition-colors animate-bounce" />
                  <div className="mt-3 flex text-xs text-slate-500">
                    <p className="font-bold text-indigo-600 hover:underline">Upload a logo file</p>
                    <p className="pl-1">or drag and drop</p>
                  </div>
                  <p className="text-[10px] text-slate-400 mt-1">PNG, JPG, GIF up to 2MB</p>
                  {logoName && (
                    <span className="text-xs font-semibold text-emerald-600 bg-emerald-50 border border-emerald-100 rounded-md px-2 py-0.5 mt-2">
                      Selected: {logoName}
                    </span>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* 2. AI Weights Tab */}
        {activeTab === "ai_weights" && (
          <div className="space-y-6 animate-in fade-in duration-200">
            <div>
              <h3 className="font-bold text-slate-900 text-base flex items-center gap-2">
                <Sliders className="w-5 h-5 text-indigo-600" />
                Algorithm Priorities
              </h3>
              <p className="text-xs text-slate-400 mt-1">
                Adjust how the AI evaluates candidate profiles. Changes here instantly recalculate dashboard stats & generate new AI Insights.
              </p>
            </div>

            {/* Custom Interactive Sliders */}
            <div className="p-5 border border-indigo-100 rounded-xl bg-indigo-50/20 space-y-6">
              
              {/* Slider 1: Skill Match */}
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <span className="text-sm font-bold text-slate-800">Skill Match Weight</span>
                  <span className="text-xs font-bold text-indigo-700 bg-indigo-100/70 rounded-full px-2.5 py-0.5 font-mono">
                    {skillWeight}%
                  </span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="100"
                  value={skillWeight}
                  onChange={(e) => setSkillWeight(Number(e.target.value))}
                  className="w-full accent-indigo-600 h-1.5 bg-slate-200 rounded-lg cursor-pointer"
                />
                <p className="text-xs text-slate-400">Prioritizes exact keyword and technical capability matching.</p>
              </div>

              {/* Slider 2: Experience */}
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <span className="text-sm font-bold text-slate-800">Experience Weight</span>
                  <span className="text-xs font-bold text-indigo-700 bg-indigo-100/70 rounded-full px-2.5 py-0.5 font-mono">
                    {experienceWeight}%
                  </span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="100"
                  value={experienceWeight}
                  onChange={(e) => setExperienceWeight(Number(e.target.value))}
                  className="w-full accent-indigo-600 h-1.5 bg-slate-200 rounded-lg cursor-pointer"
                />
                <p className="text-xs text-slate-400">Balances tenure and seniority against raw skills.</p>
              </div>

              {/* Slider 3: Cultural Fit */}
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <span className="text-sm font-bold text-slate-800">Cultural Fit Weight</span>
                  <span className="text-xs font-bold text-indigo-700 bg-indigo-100/70 rounded-full px-2.5 py-0.5 font-mono">
                    {culturalWeight}%
                  </span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="100"
                  value={culturalWeight}
                  onChange={(e) => setCulturalWeight(Number(e.target.value))}
                  className="w-full accent-indigo-600 h-1.5 bg-slate-200 rounded-lg cursor-pointer"
                />
                <p className="text-xs text-slate-400">Evaluates soft skills and behavioral indicators from cover letters.</p>
              </div>

            </div>
          </div>
        )}

        {/* 3. Integrations Tab */}
        {activeTab === "integrations" && (
          <div className="space-y-6 animate-in fade-in duration-200">
            <div>
              <h3 className="font-bold text-slate-900 text-base flex items-center gap-2">
                <Globe className="w-5 h-5 text-indigo-600" />
                Connected Services
              </h3>
              <p className="text-xs text-slate-400 mt-1">Manage secure API connections with external recruiter utilities.</p>
            </div>

            <div className="space-y-4 pt-2">
              
              {/* Google Calendar card */}
              <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between p-4 border border-slate-200 rounded-xl bg-slate-50 hover:bg-slate-100/50 transition-colors">
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 bg-white rounded-lg shadow-xs flex items-center justify-center p-2 border border-slate-100 shrink-0">
                    <Calendar className="w-6 h-6 text-indigo-600" />
                  </div>
                  <div>
                    <h4 className="font-bold text-sm text-slate-900">Google Calendar</h4>
                    <p className="text-xs text-slate-500">Used for automated interview scheduling.</p>
                  </div>
                </div>
                <div className="flex items-center gap-3 w-full md:w-auto justify-between md:justify-end mt-2 md:mt-0">
                  <span className="flex items-center gap-1 text-xs font-bold text-emerald-700 px-2.5 py-1 bg-emerald-50 border border-emerald-100 rounded-full">
                    <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span> Connected
                  </span>
                  <button className="text-xs font-bold border border-slate-200 text-slate-700 bg-white px-4 py-2 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer">
                    Manage
                  </button>
                </div>
              </div>

              {/* SendGrid Integration card */}
              <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between p-4 border border-slate-200 rounded-xl bg-slate-50 hover:bg-slate-100/50 transition-colors">
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 bg-white rounded-lg shadow-xs flex items-center justify-center p-2 border border-slate-100 shrink-0">
                    <Mail className="w-6 h-6 text-indigo-600" />
                  </div>
                  <div>
                    <h4 className="font-bold text-sm text-slate-900">SendGrid Email Delivery</h4>
                    <p className="text-xs text-slate-500">Transactional email delivery for automated pipeline logs.</p>
                  </div>
                </div>
                <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 w-full md:w-auto mt-2 md:mt-0">
                  {sendGridConnected ? (
                    <span className="flex items-center justify-center gap-1 text-xs font-bold text-emerald-700 px-2.5 py-1.5 bg-emerald-50 border border-emerald-100 rounded-full shrink-0">
                      <span className="w-1.5 h-1.5 rounded-full bg-emerald-500"></span> Connected
                    </span>
                  ) : (
                    <span className="flex items-center justify-center gap-1 text-xs font-bold text-slate-500 px-2.5 py-1.5 bg-slate-100 border border-slate-200 rounded-full shrink-0">
                      Not Connected
                    </span>
                  )}
                  
                  <div className="flex gap-2 w-full sm:w-auto">
                    <input
                      type="password"
                      value={sendGridKey}
                      onChange={(e) => setSendGridKey(e.target.value)}
                      placeholder="SendGrid API Key"
                      className="w-full sm:w-40 bg-white border border-slate-200 rounded-lg px-3 py-2 text-xs outline-none focus:border-indigo-500 transition-all"
                    />
                    <button
                      onClick={() => setSendGridConnected(!sendGridConnected)}
                      className={`text-xs font-semibold px-4 py-2 rounded-lg transition-colors cursor-pointer shrink-0 ${
                        sendGridConnected
                          ? "bg-red-50 text-red-700 hover:bg-red-100 border border-red-200"
                          : "bg-indigo-600 text-white hover:bg-indigo-700"
                      }`}
                    >
                      {sendGridConnected ? "Disconnect" : "Connect"}
                    </button>
                  </div>
                </div>
              </div>

            </div>
          </div>
        )}

        {/* 4. Notifications Settings Tab */}
        {activeTab === "notifications" && (
          <div className="space-y-6 animate-in fade-in duration-200">
            <div>
              <h3 className="font-bold text-slate-900 text-base flex items-center gap-2">
                <Bell className="w-5 h-5 text-indigo-600" />
                Email Templates
              </h3>
              <p className="text-xs text-slate-400 mt-1">Configure automated template strings sent to candidates throughout the pipeline.</p>
            </div>

            <div className="space-y-4 pt-2">
              <div className="w-full md:w-1/3">
                <label className="block text-xs font-semibold text-slate-500 mb-1">Template Type</label>
                <select
                  value={selectedTemplateType}
                  onChange={(e) => handleTemplateTypeChange(e.target.value)}
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none cursor-pointer"
                >
                  <option value="Interview Invite">Interview Invite</option>
                  <option value="Application Received">Application Received</option>
                  <option value="Rejection">Rejection</option>
                  <option value="Offer Extended">Offer Extended</option>
                </select>
              </div>

              <div className="space-y-1">
                <label className="block text-xs font-semibold text-slate-500 mb-1">Subject Line</label>
                <input
                  type="text"
                  value={subjectInput}
                  onChange={(e) => setSubjectInput(e.target.value)}
                  className="w-full border border-slate-200 rounded-lg px-4 py-2.5 text-sm outline-none focus:border-indigo-500 transition-all"
                />
              </div>

              <div className="space-y-1">
                <label className="block text-xs font-semibold text-slate-500 mb-1">Body Content (Supports Markdown)</label>
                <textarea
                  value={bodyInput}
                  onChange={(e) => setBodyInput(e.target.value)}
                  rows={8}
                  className="w-full border border-slate-200 rounded-lg px-4 py-2.5 text-xs font-mono outline-none focus:border-indigo-500 transition-all leading-relaxed"
                />
              </div>

              <div className="pt-2 flex">
                <button
                  onClick={triggerTestEmail}
                  className="text-xs font-bold text-slate-700 bg-white border border-slate-200 px-5 py-2.5 rounded-lg hover:bg-slate-50 transition-colors shadow-xs cursor-pointer"
                >
                  Test Template
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Universal Save Button at the Bottom of settings card */}
        <div className="mt-8 pt-6 border-t border-slate-200 flex justify-end">
          <button
            onClick={handleGlobalSave}
            className="bg-indigo-600 text-white font-semibold text-sm px-8 py-2.5 rounded-lg shadow-sm hover:bg-indigo-700 transition-colors cursor-pointer"
          >
            Save Changes
          </button>
        </div>

      </div>
    </div>
  );
}
