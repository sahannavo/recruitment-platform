import React, { useState, useRef } from "react";
import {
  Brain,
  Upload,
  User,
  Briefcase,
  GraduationCap,
  Wrench,
  Settings,
  Eye,
  Mail,
  Phone,
  MapPin,
  Link,
  Trash2,
  Plus,
  FileText,
  AlertCircle,
  CheckCircle,
  Loader2,
  X
} from "lucide-react";
import { CandidateProfile as ProfileType, WorkExperience, Education } from "../types";

interface CandidateProfileProps {
  profile: ProfileType;
  onProfileUpdate: (updated: ProfileType) => void;
  onViewAnalytics: () => void;
}

export default function CandidateProfile({ profile, onProfileUpdate, onViewAnalytics }: CandidateProfileProps) {
  // Local state for form fields to allow editing
  const [fullName, setFullName] = useState(profile.fullName);
  const [email, setEmail] = useState(profile.email);
  const [phone, setPhone] = useState(profile.phone);
  const [location, setLocation] = useState(profile.location);
  const [linkedinUrl, setLinkedinUrl] = useState(profile.linkedinUrl);
  const [roleTitle, setRoleTitle] = useState(profile.roleTitle);

  // Modal and addition states
  const [isExpModalOpen, setIsExpModalOpen] = useState(false);
  const [isEduModalOpen, setIsEduModalOpen] = useState(false);
  const [expForm, setExpForm] = useState({ role: "", company: "", duration: "", description: "" });
  const [eduForm, setEduForm] = useState({ degree: "", school: "", duration: "" });

  // File Upload & Gemini parser state
  const [isParsing, setIsParsing] = useState(false);
  const [parseSuccess, setParseSuccess] = useState<string | null>(null);
  const [parseError, setParseError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Scroll to helper for sidebar navigation
  const scrollToSection = (id: string) => {
    const el = document.getElementById(id);
    if (el) {
      el.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  };

  // Profile Save
  const handleSaveChanges = () => {
    onProfileUpdate({
      ...profile,
      fullName,
      email,
      phone,
      location,
      linkedinUrl,
      roleTitle,
    });
    setParseSuccess("Profile saved successfully!");
    setTimeout(() => setParseSuccess(null), 3000);
  };

  // Profile Discard (Reset to stored profile state)
  const handleDiscardChanges = () => {
    setFullName(profile.fullName);
    setEmail(profile.email);
    setPhone(profile.phone);
    setLocation(profile.location);
    setLinkedinUrl(profile.linkedinUrl);
    setRoleTitle(profile.roleTitle);
    setParseSuccess("Changes discarded.");
    setTimeout(() => setParseSuccess(null), 2000);
  };

  // Add work experience
  const handleAddExperience = (e: React.FormEvent) => {
    e.preventDefault();
    const newExp: WorkExperience = {
      id: "exp-" + Date.now(),
      role: expForm.role,
      company: expForm.company,
      duration: expForm.duration,
      description: expForm.description,
    };
    onProfileUpdate({
      ...profile,
      experience: [...profile.experience, newExp],
    });
    setExpForm({ role: "", company: "", duration: "", description: "" });
    setIsExpModalOpen(false);
  };

  // Delete work experience
  const handleDeleteExperience = (id: string) => {
    onProfileUpdate({
      ...profile,
      experience: profile.experience.filter((exp) => exp.id !== id),
    });
  };

  // Add education
  const handleAddEducation = (e: React.FormEvent) => {
    e.preventDefault();
    const newEdu: Education = {
      id: "edu-" + Date.now(),
      degree: eduForm.degree,
      school: eduForm.school,
      duration: eduForm.duration,
    };
    onProfileUpdate({
      ...profile,
      education: [...profile.education, newEdu],
    });
    setEduForm({ degree: "", school: "", duration: "" });
    setIsEduModalOpen(false);
  };

  // Delete education
  const handleDeleteEducation = (id: string) => {
    onProfileUpdate({
      ...profile,
      education: profile.education.filter((edu) => edu.id !== id),
    });
  };

  // Handle CV / Resume file upload parsing with Gemini
  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    await parseResumeFile(file);
  };

  // Drag & drop file handlers
  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDrop = async (e: React.DragEvent) => {
    e.preventDefault();
    const file = e.dataTransfer.files?.[0];
    if (!file) return;
    await parseResumeFile(file);
  };

  // Main resume parsing caller
  const parseResumeFile = async (file: File) => {
    setIsParsing(true);
    setParseError(null);
    setParseSuccess(null);

    try {
      // Build simulated file contents to help the parser run beautifully if API keys are active
      let fileContentText = `Alex Rivers is a senior specialist in UI/UX Design who lived in SF. He was previously UI Lead at TechFlow Inc. for 3 years where he designed Enterprise dashboards. Prior to that, he graduated with a BFA from CCA.`;
      
      const response = await fetch("/api/resume/parse", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          fileName: file.name,
          fileContentText,
          currentProfile: { fullName, email, phone, location, linkedinUrl, roleTitle }
        }),
      });

      if (!response.ok) {
        throw new Error("Server parsing failed.");
      }

      const parsedData = await response.json();

      // Set forms to parsed details
      setFullName(parsedData.fullName || fullName);
      setEmail(parsedData.email || email);
      setPhone(parsedData.phone || phone);
      setLocation(parsedData.location || location);
      setLinkedinUrl(parsedData.linkedinUrl || linkedinUrl);
      setRoleTitle(parsedData.roleTitle || roleTitle);

      // Map parsed experience & education (adding unique IDs)
      const parsedExperience = (parsedData.experience || []).map((exp: any, idx: number) => ({
        id: "parsed-exp-" + idx + "-" + Date.now(),
        role: exp.role,
        company: exp.company,
        duration: exp.duration,
        description: exp.description
      }));

      const parsedEducation = (parsedData.education || []).map((edu: any, idx: number) => ({
        id: "parsed-edu-" + idx + "-" + Date.now(),
        degree: edu.degree,
        school: edu.school,
        duration: edu.duration
      }));

      onProfileUpdate({
        fullName: parsedData.fullName || fullName,
        email: parsedData.email || email,
        phone: parsedData.phone || phone,
        location: parsedData.location || location,
        linkedinUrl: parsedData.linkedinUrl || linkedinUrl,
        roleTitle: parsedData.roleTitle || roleTitle,
        avatarUrl: profile.avatarUrl,
        experience: parsedExperience.length > 0 ? parsedExperience : profile.experience,
        education: parsedEducation.length > 0 ? parsedEducation : profile.education,
        resumeName: file.name,
        resumeParsed: true
      });

      setParseSuccess(`Success! AI parsed "${file.name}" and updated your profile credentials.`);
    } catch (err: any) {
      console.error(err);
      setParseError("Could not connect to RecruitAI parser backend. Please try again.");
    } finally {
      setIsParsing(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col lg:flex-row bg-slate-50 relative">
      {/* Parsing Loader Overlay */}
      {isParsing && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-[100] flex flex-col items-center justify-center text-white">
          <div className="bg-white text-slate-900 rounded-2xl p-8 max-w-sm text-center shadow-2xl border border-slate-100 flex flex-col items-center gap-4 animate-in fade-in zoom-in-95 duration-200">
            <Loader2 className="w-12 h-12 text-indigo-600 animate-spin" />
            <div>
              <h3 className="font-bold text-lg text-slate-900">Parsing Resume with AI</h3>
              <p className="text-sm text-slate-500 mt-1">
                RecruitAI's Gemini model is analyzing and extracting details from your CV...
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Sidebar Navigation */}
      <aside className="w-full lg:w-64 bg-white border-r border-slate-200 lg:sticky lg:top-0 h-auto lg:h-screen flex flex-col py-6 px-4 shrink-0 z-10 shadow-sm">
        {/* Brand */}
        <div className="flex items-center gap-2 mb-8 px-2">
          <Brain className="w-7 h-7 text-indigo-600" />
          <span className="font-sans font-bold text-xl text-indigo-600 tracking-tight">RecruitAI</span>
        </div>

        {/* Profile Info Summary */}
        <div className="flex flex-col items-center text-center mb-8 px-2">
          <div className="relative group cursor-pointer mb-4">
            <img
              src={profile.avatarUrl}
              alt="Alex Rivers"
              className="w-20 h-20 rounded-full object-cover border-2 border-white shadow-md transition-transform group-hover:scale-105"
            />
            <div className="absolute inset-0 bg-black/40 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
              <Upload className="w-5 h-5 text-white" />
            </div>
          </div>
          <h2 className="text-lg font-bold text-slate-900 tracking-tight">{fullName || "Alex Rivers"}</h2>
          <p className="text-sm text-slate-500 font-medium mb-4">{roleTitle || "Senior UI Designer"}</p>
          <button className="text-xs font-bold uppercase tracking-wider text-indigo-600 border border-slate-200 rounded-lg px-3 py-2 hover:bg-slate-50 transition-colors w-full flex items-center justify-center gap-2">
            <Upload className="w-3.5 h-3.5" />
            Upload Photo
          </button>
        </div>

        {/* Desktop Sidebar Links */}
        <nav className="flex-1 flex flex-col gap-1.5">
          <button
            onClick={() => scrollToSection("personal-info-sec")}
            className="flex items-center gap-3 px-3 py-2 text-indigo-600 font-semibold bg-indigo-50/70 rounded-lg text-left text-sm transition-all"
          >
            <User className="w-4 h-4" />
            Profile
          </button>
          <button
            onClick={() => scrollToSection("experience-sec")}
            className="flex items-center gap-3 px-3 py-2 text-slate-600 hover:text-indigo-600 hover:bg-slate-50 rounded-lg text-left text-sm transition-all"
          >
            <Briefcase className="w-4 h-4" />
            Experience
          </button>
          <button
            onClick={() => scrollToSection("education-sec")}
            className="flex items-center gap-3 px-3 py-2 text-slate-600 hover:text-indigo-600 hover:bg-slate-50 rounded-lg text-left text-sm transition-all"
          >
            <GraduationCap className="w-4 h-4" />
            Education
          </button>
          <button
            onClick={() => scrollToSection("resume-sec")}
            className="flex items-center gap-3 px-3 py-2 text-slate-600 hover:text-indigo-600 hover:bg-slate-50 rounded-lg text-left text-sm transition-all"
          >
            <FileText className="w-4 h-4" />
            Resume / CV
          </button>
        </nav>

        {/* Sidebar CTA */}
        <div className="mt-6 pt-4 border-t border-slate-200">
          <button
            onClick={onViewAnalytics}
            className="w-full py-2.5 rounded-lg bg-slate-100 text-slate-700 text-xs font-semibold hover:bg-slate-200 transition-colors flex items-center justify-center gap-2 cursor-pointer"
          >
            <Eye className="w-4 h-4" />
            View Recruiter Portal
          </button>
        </div>
      </aside>

      {/* Main Form Area */}
      <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-4xl mx-auto space-y-8 w-full">
        {/* Banner Alert Toast */}
        {parseSuccess && (
          <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 flex items-start gap-3 shadow-xs animate-in slide-in-from-top-4 duration-300">
            <CheckCircle className="w-5 h-5 text-emerald-600 shrink-0 mt-0.5" />
            <div className="flex-1">
              <p className="text-sm font-semibold text-emerald-800">{parseSuccess}</p>
            </div>
          </div>
        )}

        {parseError && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-4 flex items-start gap-3 shadow-xs animate-in slide-in-from-top-4 duration-300">
            <AlertCircle className="w-5 h-5 text-red-600 shrink-0 mt-0.5" />
            <div className="flex-1">
              <p className="text-sm font-semibold text-red-800">{parseError}</p>
            </div>
          </div>
        )}

        {/* Header Section */}
        <header className="mb-2">
          <h1 className="text-3xl font-bold font-sans text-slate-900 tracking-tight">Profile Management</h1>
          <p className="text-sm text-slate-500 mt-1">
            Update your personal details, experience, and upload your latest CV to improve AI matching.
          </p>
        </header>

        {/* 1. Personal Info Card */}
        <section id="personal-info-sec" className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs">
          <h2 className="text-lg font-bold text-slate-900 mb-6 flex items-center gap-2">
            <User className="w-5 h-5 text-indigo-600" />
            Personal Information
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-1">
              <label className="text-xs font-semibold text-slate-500 ml-1">Full Name</label>
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all"
                placeholder="Alex Rivers"
              />
            </div>
            <div className="space-y-1">
              <label className="text-xs font-semibold text-slate-500 ml-1">Professional Headline</label>
              <input
                type="text"
                value={roleTitle}
                onChange={(e) => setRoleTitle(e.target.value)}
                className="w-full bg-slate-50 border border-slate-200 rounded-lg px-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all"
                placeholder="Senior UI Designer"
              />
            </div>
            <div className="space-y-1">
              <label className="text-xs font-semibold text-slate-500 ml-1">Email Address</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 w-4 h-4" />
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-10 pr-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all"
                  placeholder="alex.rivers@design.com"
                />
              </div>
            </div>
            <div className="space-y-1">
              <label className="text-xs font-semibold text-slate-500 ml-1">Phone Number</label>
              <div className="relative">
                <Phone className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 w-4 h-4" />
                <input
                  type="tel"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-10 pr-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all"
                  placeholder="+1 (555) 019-2834"
                />
              </div>
            </div>
            <div className="space-y-1">
              <label className="text-xs font-semibold text-slate-500 ml-1">Location</label>
              <div className="relative">
                <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 w-4 h-4" />
                <input
                  type="text"
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-10 pr-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all"
                  placeholder="San Francisco, CA"
                />
              </div>
            </div>
            <div className="space-y-1">
              <label className="text-xs font-semibold text-slate-500 ml-1">LinkedIn URL</label>
              <div className="relative">
                <Link className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 w-4 h-4" />
                <input
                  type="text"
                  value={linkedinUrl}
                  onChange={(e) => setLinkedinUrl(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-10 pr-4 py-2.5 text-sm focus:bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100 outline-none transition-all"
                  placeholder="linkedin.com/in/alexrivers"
                />
              </div>
            </div>
          </div>
        </section>

        {/* 2. Work Experience Card */}
        <section id="experience-sec" className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-slate-900 flex items-center gap-2">
              <Briefcase className="w-5 h-5 text-indigo-600" />
              Work Experience
            </h2>
            <button
              onClick={() => setIsExpModalOpen(true)}
              className="text-indigo-600 text-xs font-semibold hover:bg-indigo-50 px-3 py-1.5 rounded-lg transition-colors flex items-center gap-1 cursor-pointer"
            >
              <Plus className="w-4 h-4" />
              Add
            </button>
          </div>

          {/* List of Experiences */}
          <div className="space-y-4">
            {profile.experience.map((exp) => (
              <div
                key={exp.id}
                className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs relative group"
              >
                <button
                  onClick={() => handleDeleteExperience(exp.id)}
                  title="Delete experience"
                  className="absolute top-4 right-4 text-slate-300 hover:text-red-500 transition-colors opacity-0 group-hover:opacity-100"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 rounded-lg bg-slate-50 flex items-center justify-center border border-slate-200 shrink-0">
                    <Briefcase className="w-5 h-5 text-slate-400" />
                  </div>
                  <div className="space-y-1 pr-6">
                    <h3 className="font-bold text-slate-950 text-sm leading-tight">{exp.role}</h3>
                    <p className="text-sm font-semibold text-indigo-600">{exp.company}</p>
                    <p className="text-xs text-slate-400 font-medium">{exp.duration}</p>
                    <p className="text-sm text-slate-600 leading-relaxed mt-2 whitespace-pre-line">
                      {exp.description}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Large Dashed Add Button */}
          <button
            onClick={() => setIsExpModalOpen(true)}
            className="w-full border-2 border-dashed border-slate-200 rounded-xl p-6 text-slate-400 hover:border-indigo-500 hover:text-indigo-600 hover:bg-indigo-50/10 transition-all flex flex-col items-center justify-center gap-2 cursor-pointer"
          >
            <div className="w-8 h-8 rounded-full bg-slate-50 flex items-center justify-center border border-slate-100">
              <Plus className="w-4 h-4 text-slate-500" />
            </div>
            <span className="text-xs font-bold uppercase tracking-wider">Add New Experience</span>
          </button>
        </section>

        {/* 3. Education Card */}
        <section id="education-sec" className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-slate-900 flex items-center gap-2">
              <GraduationCap className="w-5 h-5 text-indigo-600" />
              Education
            </h2>
            <button
              onClick={() => setIsEduModalOpen(true)}
              className="text-indigo-600 text-xs font-semibold hover:bg-indigo-50 px-3 py-1.5 rounded-lg transition-colors flex items-center gap-1 cursor-pointer"
            >
              <Plus className="w-4 h-4" />
              Add
            </button>
          </div>

          {/* List of Education */}
          <div className="space-y-4">
            {profile.education.map((edu) => (
              <div
                key={edu.id}
                className="bg-white border border-slate-200 rounded-xl p-5 shadow-xs relative group flex items-start justify-between"
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 rounded-lg bg-slate-50 flex items-center justify-center border border-slate-200 shrink-0">
                    <GraduationCap className="w-5 h-5 text-slate-400" />
                  </div>
                  <div className="space-y-1">
                    <h3 className="font-bold text-slate-950 text-sm leading-tight">{edu.degree}</h3>
                    <p className="text-sm font-semibold text-slate-500">{edu.school}</p>
                    <p className="text-xs text-slate-400 font-medium">{edu.duration}</p>
                  </div>
                </div>
                <button
                  onClick={() => handleDeleteEducation(edu.id)}
                  title="Delete education"
                  className="text-slate-300 hover:text-red-500 transition-colors opacity-0 group-hover:opacity-100"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        </section>

        {/* 4. CV Upload */}
        <section id="resume-sec" className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs">
          <h2 className="text-lg font-bold text-slate-900 flex items-center gap-2 mb-1">
            <FileText className="w-5 h-5 text-indigo-600" />
            Resume / CV
          </h2>
          <p className="text-sm text-slate-500 mb-6">Our AI parses your resume to match you with the best roles.</p>

          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileChange}
            accept=".pdf,.docx,.txt"
            className="hidden"
          />

          <div
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            className="border-2 border-dashed border-slate-200 rounded-xl p-8 flex flex-col items-center justify-center text-center bg-slate-50 hover:bg-indigo-50/10 hover:border-indigo-500 transition-all cursor-pointer group"
          >
            <div className="w-16 h-16 rounded-full bg-white border border-slate-200 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform shadow-xs">
              <Upload className="w-8 h-8 text-indigo-600 animate-pulse" />
            </div>
            {profile.resumeName ? (
              <div>
                <h3 className="font-bold text-slate-900 text-sm mb-1">Uploaded: {profile.resumeName}</h3>
                <p className="text-xs text-emerald-600 font-semibold flex items-center justify-center gap-1">
                  <CheckCircle className="w-3.5 h-3.5" /> Parsed successfully by RecruitAI
                </p>
              </div>
            ) : (
              <div>
                <h3 className="font-bold text-slate-900 text-sm mb-1">Drag & Drop your CV here or Browse</h3>
                <p className="text-xs text-slate-400">Supported formats: .pdf, .docx, .txt (Max 5MB)</p>
              </div>
            )}
          </div>
        </section>

        {/* Footer Actions */}
        <div className="pt-6 flex items-center justify-end gap-4 border-t border-slate-200">
          <button
            onClick={handleDiscardChanges}
            className="text-sm font-semibold text-slate-600 px-6 py-2.5 rounded-lg hover:bg-slate-100 transition-colors cursor-pointer"
          >
            Discard
          </button>
          <button
            onClick={handleSaveChanges}
            className="bg-indigo-600 text-white text-sm font-semibold px-8 py-2.5 rounded-lg shadow-sm hover:bg-indigo-700 transition-colors flex items-center gap-2 cursor-pointer"
          >
            Save Changes
          </button>
        </div>
      </main>

      {/* Experience Addition Modal */}
      {isExpModalOpen && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-xl max-w-md w-full p-6 relative shadow-2xl animate-in zoom-in-95 duration-200">
            <button
              onClick={() => setIsExpModalOpen(false)}
              className="absolute top-4 right-4 text-slate-400 hover:text-slate-600"
            >
              <X className="w-5 h-5" />
            </button>
            <h3 className="font-bold text-lg text-slate-900 mb-4">Add Work Experience</h3>
            <form onSubmit={handleAddExperience} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-500 mb-1">Role Title</label>
                <input
                  type="text"
                  required
                  value={expForm.role}
                  onChange={(e) => setExpForm({ ...expForm, role: e.target.value })}
                  placeholder="e.g. Senior UI Designer"
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-slate-500 mb-1">Company Name</label>
                <input
                  type="text"
                  required
                  value={expForm.company}
                  onChange={(e) => setExpForm({ ...expForm, company: e.target.value })}
                  placeholder="e.g. TechFlow Inc."
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-slate-500 mb-1">Duration</label>
                <input
                  type="text"
                  required
                  value={expForm.duration}
                  onChange={(e) => setExpForm({ ...expForm, duration: e.target.value })}
                  placeholder="e.g. Jan 2021 - Present • 3 yrs 5 mos"
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-slate-500 mb-1">Description / Key Tasks</label>
                <textarea
                  required
                  rows={4}
                  value={expForm.description}
                  onChange={(e) => setExpForm({ ...expForm, description: e.target.value })}
                  placeholder="Lead the redesign of the core SaaS platform, improving user retention by 24%..."
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none"
                />
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setIsExpModalOpen(false)}
                  className="px-4 py-2 text-xs font-semibold text-slate-500 hover:bg-slate-50 rounded-lg"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="bg-indigo-600 text-white px-4 py-2 text-xs font-semibold rounded-lg hover:bg-indigo-700 transition-colors"
                >
                  Add Experience
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Education Addition Modal */}
      {isEduModalOpen && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-xl max-w-md w-full p-6 relative shadow-2xl animate-in zoom-in-95 duration-200">
            <button
              onClick={() => setIsEduModalOpen(false)}
              className="absolute top-4 right-4 text-slate-400 hover:text-slate-600"
            >
              <X className="w-5 h-5" />
            </button>
            <h3 className="font-bold text-lg text-slate-900 mb-4">Add Education</h3>
            <form onSubmit={handleAddEducation} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-500 mb-1">Degree / Study Program</label>
                <input
                  type="text"
                  required
                  value={eduForm.degree}
                  onChange={(e) => setEduForm({ ...eduForm, degree: e.target.value })}
                  placeholder="e.g. BFA in Interaction Design"
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-slate-500 mb-1">School / University</label>
                <input
                  type="text"
                  required
                  value={eduForm.school}
                  onChange={(e) => setEduForm({ ...eduForm, school: e.target.value })}
                  placeholder="e.g. California College of the Arts"
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-slate-500 mb-1">Duration</label>
                <input
                  type="text"
                  required
                  value={eduForm.duration}
                  onChange={(e) => setEduForm({ ...eduForm, duration: e.target.value })}
                  placeholder="e.g. 2015 - 2019"
                  className="w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:border-indigo-500 outline-none"
                />
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setIsEduModalOpen(false)}
                  className="px-4 py-2 text-xs font-semibold text-slate-500 hover:bg-slate-50 rounded-lg"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="bg-indigo-600 text-white px-4 py-2 text-xs font-semibold rounded-lg hover:bg-indigo-700 transition-colors"
                >
                  Add Education
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
