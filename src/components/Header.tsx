import { Search, Bell, Settings, ArrowLeftRight } from "lucide-react";
import { AppPortal } from "../types";

interface HeaderProps {
  currentPortal: AppPortal;
  onPortalChange: (portal: AppPortal) => void;
  companyName: string;
}

export default function Header({ currentPortal, onPortalChange, companyName }: HeaderProps) {
  return (
    <header className="bg-white border-b border-gray-200 sticky top-0 z-50 shadow-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        {/* Left Side: Brand and Links */}
        <div className="flex items-center gap-8">
          <div className="flex items-center gap-2 cursor-pointer" onClick={() => onPortalChange("recruiter_analytics")}>
            <div className="w-8 h-8 rounded-lg bg-indigo-600 flex items-center justify-center text-white font-bold text-lg">
              R
            </div>
            <span className="font-sans font-bold text-xl text-indigo-600 tracking-tight transition-all">
              {currentPortal === "recruiter_settings" ? companyName : "RecruitAI"}
            </span>
          </div>

          {/* Web Navigation */}
          <nav className="hidden md:flex items-center space-x-1 h-16">
            <button
              onClick={() => onPortalChange("recruiter_analytics")}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors h-full flex items-center border-b-2 ${
                currentPortal === "recruiter_analytics"
                  ? "text-indigo-600 border-indigo-600 font-semibold"
                  : "text-gray-500 border-transparent hover:text-gray-900"
              }`}
            >
              Dashboard
            </button>
            <button className="px-3 py-2 text-sm font-medium text-gray-500 border-b-2 border-transparent hover:text-gray-900 h-full flex items-center">
              Jobs
            </button>
            <button
              onClick={() => onPortalChange("candidate")}
              className={`px-3 py-2 text-sm font-medium rounded-md transition-colors h-full flex items-center border-b-2 ${
                currentPortal === "candidate"
                  ? "text-indigo-600 border-indigo-600 font-semibold"
                  : "text-gray-500 border-transparent hover:text-gray-900"
              }`}
            >
              Applications
            </button>
            <button className="px-3 py-2 text-sm font-medium text-gray-500 border-b-2 border-transparent hover:text-gray-900 h-full flex items-center">
              Messages
            </button>
          </nav>
        </div>

        {/* Right Side: Search, Dynamic Portal Quick Switch, Notifications, Settings, Avatar */}
        <div className="flex items-center gap-4">
          {/* Quick Portal Switcher (Elegant high-contrast badge) */}
          <div className="bg-indigo-50 border border-indigo-100 rounded-full px-3 py-1 hidden lg:flex items-center gap-2 shadow-xs">
            <span className="text-xs font-semibold text-indigo-700">Portal View:</span>
            <select
              value={currentPortal}
              onChange={(e) => onPortalChange(e.target.value as AppPortal)}
              className="bg-transparent text-xs text-indigo-900 font-bold focus:outline-none cursor-pointer pr-1"
            >
              <option value="candidate">Candidate Profile</option>
              <option value="recruiter_analytics">Recruiter Analytics</option>
              <option value="recruiter_settings">Admin Settings</option>
            </select>
          </div>

          {/* Search Box */}
          <div className="hidden sm:flex items-center bg-gray-50 border border-gray-200 rounded-full px-3 py-1.5 focus-within:ring-2 focus-within:ring-indigo-100 focus-within:border-indigo-500 transition-all">
            <Search className="w-4 h-4 text-gray-400 mr-2" />
            <input
              type="text"
              placeholder="Search..."
              className="bg-transparent border-none text-sm text-gray-800 placeholder-gray-400 focus:ring-0 outline-none w-32 md:w-48 p-0"
            />
          </div>

          {/* Notifications button */}
          <button className="relative p-2 text-gray-500 hover:text-indigo-600 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer">
            <Bell className="w-5 h-5" />
            <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-indigo-600 rounded-full"></span>
          </button>

          {/* Settings button */}
          <button
            onClick={() => onPortalChange("recruiter_settings")}
            className={`p-2 rounded-lg transition-all cursor-pointer ${
              currentPortal === "recruiter_settings"
                ? "text-indigo-600 bg-indigo-50 border border-indigo-100"
                : "text-gray-500 hover:text-indigo-600 hover:bg-gray-100"
            }`}
          >
            <Settings className="w-5 h-5" />
          </button>

          {/* Avatar (Click to open Candidate profile portal) */}
          <div
            onClick={() => onPortalChange("candidate")}
            title="View Candidate Portal"
            className="w-8 h-8 rounded-full overflow-hidden border border-gray-200 cursor-pointer hover:ring-2 hover:ring-indigo-500 transition-all shrink-0 shadow-sm"
          >
            <img
              src="https://lh3.googleusercontent.com/aida-public/AB6AXuAPcXjPinq21DzokgVaxRAV-T9EUfxbr5qiERIVihvgrpoEXi15qJmhl590x3s1P3InmEtIbMYiMG0unb-pMruZtTzhbDugGXpN_-usPYcmwFZ1lkmHTqWvH568X96Oq90BR32so7EwvxvCi8jyJSJn4dg73b357B-DweaRp2XR56Dy4PhwhU5XOl_sSDLllM2omHiqXR5CXDXU6kzSbmFIa04kZLRAQTYBkIzqME-bD4VLu1ckh4RWLI4haNYq8blg6VOwbZjA2Bw7"
              alt="Alex Rivers Avatar"
              className="w-full h-full object-cover"
              referrerPolicy="no-referrer"
            />
          </div>
        </div>
      </div>

      {/* Mobile Portal Navigation Bar */}
      <div className="md:hidden flex items-center justify-around py-2 border-t border-gray-100 bg-gray-50">
        <button
          onClick={() => onPortalChange("candidate")}
          className={`px-3 py-1 text-xs font-semibold rounded-full transition-colors ${
            currentPortal === "candidate" ? "bg-indigo-600 text-white" : "text-gray-600 hover:bg-gray-100"
          }`}
        >
          Candidate
        </button>
        <button
          onClick={() => onPortalChange("recruiter_analytics")}
          className={`px-3 py-1 text-xs font-semibold rounded-full transition-colors ${
            currentPortal === "recruiter_analytics" ? "bg-indigo-600 text-white" : "text-gray-600 hover:bg-gray-100"
          }`}
        >
          Analytics
        </button>
        <button
          onClick={() => onPortalChange("recruiter_settings")}
          className={`px-3 py-1 text-xs font-semibold rounded-full transition-colors ${
            currentPortal === "recruiter_settings" ? "bg-indigo-600 text-white" : "text-gray-600 hover:bg-gray-100"
          }`}
        >
          Settings
        </button>
      </div>
    </header>
  );
}
