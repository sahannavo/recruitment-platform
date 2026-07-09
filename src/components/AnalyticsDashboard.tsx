import React, { useState, useEffect } from "react";
import {
  Calendar,
  Download,
  TrendingUp,
  Sparkles,
  MoreHorizontal,
  PieChart,
  BarChart2,
  Loader2,
} from "lucide-react";
import { AdminSettingsState } from "../types";

interface AnalyticsDashboardProps {
  settings: AdminSettingsState;
}

export default function AnalyticsDashboard({ settings }: AnalyticsDashboardProps) {
  // Date Picker local states
  const [startDate, setStartDate] = useState("2023-09-01");
  const [endDate, setEndDate] = useState("2023-10-31");
  const [appliedRange, setAppliedRange] = useState("09/01/2023 - 10/31/2023");

  // AI Generated Insight state
  const [aiInsight, setAiInsight] = useState<string>(
    '"Candidates progressing through the Engineering pipeline are accepting offers 12% faster than the historical average."'
  );
  const [isLoadingInsight, setIsLoadingInsight] = useState(false);

  // Fetch or simulate AI Insight from Gemini when Settings Weights are adjusted (with 600ms debounce to prevent API rate limiting)
  useEffect(() => {
    let active = true;
    const delayDebounceFn = setTimeout(async () => {
      setIsLoadingInsight(true);
      try {
        const response = await fetch("/api/insights/generate", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            skillWeight: settings.skillWeight,
            experienceWeight: settings.experienceWeight,
            culturalWeight: settings.culturalWeight,
          }),
        });

        if (response.ok && active) {
          const data = await response.json();
          setAiInsight(data.insight);
        }
      } catch (error) {
        console.warn("Failed to generate dynamic AI insight:", error);
      } finally {
        if (active) {
          setIsLoadingInsight(false);
        }
      }
    }, 600);

    return () => {
      active = false;
      clearTimeout(delayDebounceFn);
    };
  }, [settings.skillWeight, settings.experienceWeight, settings.culturalWeight]);

  const handleApplyDates = (e: React.FormEvent) => {
    e.preventDefault();
    // Reformat dates for aesthetic presentation
    const format = (dStr: string) => {
      const [y, m, d] = dStr.split("-");
      return m && d && y ? `${m}/${d}/${y}` : dStr;
    };
    setAppliedRange(`${format(startDate)} - ${format(endDate)}`);
  };

  return (
    <div className="space-y-6">
      {/* Top Controls Bar */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold font-sans text-slate-900 tracking-tight">Analytics Reports</h1>
          <p className="text-sm text-slate-500 mt-1">Insights and performance metrics for your recruitment pipeline.</p>
        </div>

        {/* Date Filter Controls */}
        <form onSubmit={handleApplyDates} className="flex flex-col sm:flex-row items-center gap-3 w-full md:w-auto">
          <div className="flex items-center bg-white border border-slate-200 rounded-lg overflow-hidden shadow-xs h-10 w-full sm:w-auto focus-within:ring-2 focus-within:ring-indigo-500/20 focus-within:border-indigo-500 transition-all">
            <div className="flex items-center px-3 text-slate-400 bg-slate-50 border-r border-slate-200 h-full shrink-0">
              <Calendar className="w-4 h-4" />
            </div>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="border-none text-xs font-bold text-slate-700 bg-transparent w-28 px-2 h-full focus:ring-0 outline-none"
            />
            <span className="text-slate-400 text-xs px-1">-</span>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="border-none text-xs font-bold text-slate-700 bg-transparent w-28 px-2 h-full focus:ring-0 outline-none"
            />
            <button
              type="submit"
              className="bg-indigo-600 text-white font-semibold text-xs px-4 h-full hover:bg-indigo-700 transition-colors cursor-pointer shrink-0"
            >
              Apply
            </button>
          </div>

          <button
            type="button"
            className="flex items-center justify-center gap-2 border border-slate-200 text-slate-600 font-bold text-xs px-4 h-10 rounded-lg bg-white hover:bg-slate-50 transition-colors shadow-xs w-full sm:w-auto active:scale-98 transition-transform cursor-pointer"
          >
            <Download className="w-4 h-4" />
            Export Report
          </button>
        </form>
      </div>

      {/* Date display banner */}
      <div className="text-xs font-bold text-slate-500 bg-slate-100 rounded-lg px-4 py-2 inline-flex items-center gap-1.5 shadow-xs">
        <span className="w-2 h-2 rounded-full bg-indigo-500 animate-pulse"></span>
        Active Analysis Range: <span className="text-indigo-700">{appliedRange}</span>
      </div>

      {/* Grid Content */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-2">
        
        {/* Card 1: Time-to-Hire Trend (Line Chart) */}
        <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs hover:shadow-md transition-shadow flex flex-col h-[380px]">
          <div className="flex justify-between items-start mb-4">
            <div>
              <h2 className="font-bold text-slate-900 text-base leading-tight">Time-to-Hire Trend</h2>
              <p className="text-xs text-slate-400 mt-1">Average days from application to offer.</p>
            </div>
            <button className="text-slate-400 hover:text-indigo-600 transition-colors">
              <MoreHorizontal className="w-5 h-5" />
            </button>
          </div>

          {/* Interactive SVG Chart Container */}
          <div className="flex-1 w-full bg-slate-50 rounded-lg border border-slate-100 flex items-end p-4 gap-2 relative overflow-hidden">
            
            {/* Custom Responsive SVG Curve Line */}
            <svg className="absolute inset-0 w-full h-[calc(100%-3rem)]" preserveAspectRatio="none" viewBox="0 0 100 100">
              {/* Fill area beneath the line */}
              <path
                d="M 0 85 Q 25 68, 50 72 T 100 25 L 100 100 L 0 100 Z"
                fill="url(#indigo-gradient-area)"
                opacity="0.15"
              />
              {/* Stroke line */}
              <path
                d="M 0 85 Q 25 68, 50 72 T 100 25"
                fill="none"
                stroke="#4f46e5"
                strokeWidth="2.5"
                strokeLinecap="round"
              />
              
              {/* Chart Grid Lines */}
              <line x1="0" y1="20" x2="100" y2="20" stroke="#e2e8f0" strokeDasharray="3 3" strokeWidth="0.5" />
              <line x1="0" y1="50" x2="100" y2="50" stroke="#e2e8f0" strokeDasharray="3 3" strokeWidth="0.5" />
              <line x1="0" y1="80" x2="100" y2="80" stroke="#e2e8f0" strokeDasharray="3 3" strokeWidth="0.5" />

              {/* Data Interactive Dots */}
              <circle cx="25" cy="74.5" r="3.5" fill="#4f46e5" stroke="#ffffff" strokeWidth="1.5" />
              <circle cx="50" cy="72" r="3.5" fill="#4f46e5" stroke="#ffffff" strokeWidth="1.5" />
              <circle cx="75" cy="48.5" r="3.5" fill="#4f46e5" stroke="#ffffff" strokeWidth="1.5" />
              <circle cx="100" cy="25" r="4" fill="#4f46e5" stroke="#ffffff" strokeWidth="2" />

              <defs>
                <linearGradient id="indigo-gradient-area" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#4f46e5" />
                  <stop offset="100%" stopColor="#ffffff" stopOpacity="0" />
                </linearGradient>
              </defs>
            </svg>

            {/* Y Axis Labels */}
            <div className="absolute left-4 top-4 bottom-14 flex flex-col justify-between text-[10px] font-bold text-slate-400 font-mono">
              <span>40d</span>
              <span>30d</span>
              <span>20d</span>
              <span>10d</span>
            </div>

            {/* X Axis Labels */}
            <div className="absolute left-10 right-4 bottom-2 flex justify-between text-[10px] font-bold text-slate-400 uppercase tracking-wider w-[calc(100%-3.5rem)] border-t border-slate-200 pt-2">
              <span>Jun</span>
              <span>Jul</span>
              <span>Aug</span>
              <span>Sep</span>
              <span>Oct</span>
            </div>
          </div>
        </div>

        {/* Card 2: Applicant Sources (Donut Chart) */}
        <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs hover:shadow-md transition-shadow flex flex-col h-[380px]">
          <div className="flex justify-between items-start mb-4">
            <div>
              <h2 className="font-bold text-slate-900 text-base leading-tight">Applicant Sources</h2>
              <p className="text-xs text-slate-400 mt-1">Where successful candidates originate.</p>
            </div>
            <button className="text-slate-400 hover:text-indigo-600 transition-colors">
              <PieChart className="w-5 h-5" />
            </button>
          </div>

          <div className="flex-1 flex flex-col sm:flex-row items-center justify-center gap-6">
            {/* Donut Chart SVG */}
            <div className="relative w-40 h-40 shrink-0">
              <svg className="w-full h-full transform -rotate-90" viewBox="0 0 100 100">
                {/* Background circle */}
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="#f1f5f9" strokeWidth="12" />
                {/* Website 15% (Orange) */}
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="#f59e0b" strokeWidth="12" strokeDasharray="251.2" strokeDashoffset="213.52" />
                {/* Referrals 25% (Green) */}
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="#10b981" strokeWidth="12" strokeDasharray="251.2" strokeDashoffset="188.4" transform="rotate(54 50 50)" />
                {/* Job Boards 20% (Navy) */}
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="#1e293b" strokeWidth="12" strokeDasharray="251.2" strokeDashoffset="200.96" transform="rotate(144 50 50)" />
                {/* LinkedIn 40% (Indigo) */}
                <circle cx="50" cy="50" r="40" fill="transparent" stroke="#4f46e5" strokeWidth="12" strokeDasharray="251.2" strokeDashoffset="150.72" transform="rotate(216 50 50)" />
              </svg>
              <div className="absolute inset-0 flex items-center justify-center flex-col">
                <span className="font-sans font-bold text-3xl text-slate-900 tracking-tight">642</span>
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Total</span>
              </div>
            </div>

            {/* Legend details */}
            <div className="flex flex-col gap-2 w-full sm:w-auto">
              <div className="flex items-center justify-between gap-6 p-2 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full bg-indigo-600"></div>
                  <span className="text-sm font-semibold text-slate-700">LinkedIn</span>
                </div>
                <span className="text-sm font-bold text-slate-900">40%</span>
              </div>
              <div className="flex items-center justify-between gap-6 p-2 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full bg-emerald-500"></div>
                  <span className="text-sm font-semibold text-slate-700">Referrals</span>
                </div>
                <span className="text-sm font-bold text-slate-900">25%</span>
              </div>
              <div className="flex items-center justify-between gap-6 p-2 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full bg-slate-800"></div>
                  <span className="text-sm font-semibold text-slate-700">Job Boards</span>
                </div>
                <span className="text-sm font-bold text-slate-900">20%</span>
              </div>
              <div className="flex items-center justify-between gap-6 p-2 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full bg-amber-500"></div>
                  <span className="text-sm font-semibold text-slate-700">Website</span>
                </div>
                <span className="text-sm font-bold text-slate-900">15%</span>
              </div>
            </div>
          </div>
        </div>

        {/* Card 3: Applications per Department */}
        <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-xs hover:shadow-md transition-shadow flex flex-col h-[380px]">
          <div className="flex justify-between items-start mb-4">
            <div>
              <h2 className="font-bold text-slate-900 text-base leading-tight">Applications by Department</h2>
              <p className="text-xs text-slate-400 mt-1">Volume breakdown across major units.</p>
            </div>
            <button className="text-slate-400 hover:text-indigo-600 transition-colors">
              <BarChart2 className="w-5 h-5" />
            </button>
          </div>

          <div className="flex-1 flex flex-col justify-around py-2 gap-2">
            {/* Eng */}
            <div className="space-y-1">
              <div className="flex justify-between text-xs font-semibold text-slate-600">
                <span>Engineering</span>
                <span className="font-bold text-slate-900">342</span>
              </div>
              <div className="h-4 w-full bg-slate-100 rounded-full overflow-hidden">
                <div className="h-full bg-indigo-600 rounded-full" style={{ width: "85%" }}></div>
              </div>
            </div>
            {/* Sales */}
            <div className="space-y-1">
              <div className="flex justify-between text-xs font-semibold text-slate-600">
                <span>Sales</span>
                <span className="font-bold text-slate-900">261</span>
              </div>
              <div className="h-4 w-full bg-slate-100 rounded-full overflow-hidden">
                <div className="h-full bg-indigo-400 rounded-full" style={{ width: "65%" }}></div>
              </div>
            </div>
            {/* Marketing */}
            <div className="space-y-1">
              <div className="flex justify-between text-xs font-semibold text-slate-600">
                <span>Marketing</span>
                <span className="font-bold text-slate-900">184</span>
              </div>
              <div className="h-4 w-full bg-slate-100 rounded-full overflow-hidden">
                <div className="h-full bg-indigo-300 rounded-full" style={{ width: "45%" }}></div>
              </div>
            </div>
            {/* Product */}
            <div className="space-y-1">
              <div className="flex justify-between text-xs font-semibold text-slate-600">
                <span>Product</span>
                <span className="font-bold text-slate-900">120</span>
              </div>
              <div className="h-4 w-full bg-slate-100 rounded-full overflow-hidden">
                <div className="h-full bg-indigo-200 rounded-full" style={{ width: "35%" }}></div>
              </div>
            </div>
            {/* HR */}
            <div className="space-y-1">
              <div className="flex justify-between text-xs font-semibold text-slate-600">
                <span>HR</span>
                <span className="font-bold text-slate-900">86</span>
              </div>
              <div className="h-4 w-full bg-slate-100 rounded-full overflow-hidden">
                <div className="h-full bg-indigo-100 rounded-full" style={{ width: "20%" }}></div>
              </div>
            </div>
          </div>
        </div>

        {/* Card 4: KPI & AI Insight (Dynamic Gradient Border) */}
        <div className="p-[1px] bg-gradient-to-br from-indigo-500 via-indigo-400 to-purple-400 rounded-xl shadow-xs hover:shadow-md transition-shadow flex flex-col h-[380px]">
          <div className="bg-white rounded-[11px] p-6 flex-1 flex flex-col h-full relative overflow-hidden">
            
            {/* Header */}
            <div className="flex justify-between items-start mb-4">
              <div>
                <h2 className="font-bold text-slate-900 text-base leading-tight flex items-center gap-1.5">
                  <Sparkles className="w-5 h-5 text-indigo-600 animate-pulse" />
                  Offer Acceptance Rate
                </h2>
                <p className="text-xs text-slate-400 mt-1">AI-predicted conversion likelihood.</p>
              </div>
            </div>

            {/* Main KPI Stat */}
            <div className="flex-1 flex flex-col justify-center items-center relative py-4">
              <div className="font-sans text-7xl font-extrabold text-slate-900 tracking-tighter flex items-baseline">
                84<span className="text-3xl text-slate-400 ml-1">%</span>
              </div>
              <div className="flex items-center gap-1.5 mt-4 bg-emerald-50 border border-emerald-100 px-4 py-1.5 rounded-full shadow-xs">
                <TrendingUp className="w-4 h-4 text-emerald-600" />
                <span className="text-xs font-bold text-emerald-700">+4.2%</span>
                <span className="text-xs text-slate-500 font-semibold ml-1">vs last month</span>
              </div>
            </div>

            {/* AI Generated Insight Text Box (Generative via Express + Gemini Proxy) */}
            <div className="mt-auto p-4 bg-indigo-50/50 rounded-xl border border-indigo-100 text-center min-h-[96px] flex items-center justify-center">
              {isLoadingInsight ? (
                <div className="flex flex-col items-center justify-center gap-1">
                  <Loader2 className="w-5 h-5 text-indigo-600 animate-spin" />
                  <span className="text-[10px] font-bold text-indigo-500 uppercase tracking-widest">
                    AI Analyst Recalculating...
                  </span>
                </div>
              ) : (
                <p className="text-sm text-slate-600 italic font-medium leading-relaxed">
                  {aiInsight}
                </p>
              )}
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}
