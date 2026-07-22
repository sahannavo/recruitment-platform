using System;

namespace RecruitmentAPI.Services.Notification
{
    /// <summary>
    /// Simple, configurable HTML templates for outbound notifications.
    /// In production these could be moved to the database or a template file store
    /// so non-developers can edit copy without a redeploy.
    /// </summary>
    public static class NotificationTemplates
    {
        /// <summary>Generates the subject line for an interview reminder email.</summary>
        /// <param name="jobTitle">The title of the job position.</param>
        /// <returns>The formatted email subject.</returns>
        public static string InterviewReminderSubject(string jobTitle) =>
            $"Interview Reminder: {jobTitle}";

        /// <summary>Generates the HTML body for an interview reminder email.</summary>
        /// <param name="candidateName">The candidate's full name.</param>
        /// <param name="jobTitle">The title of the job position.</param>
        /// <param name="scheduledAt">The scheduled interview date/time.</param>
        /// <param name="meetingLink">The URL for the virtual meeting.</param>
        /// <returns>The formatted HTML email body.</returns>
        public static string InterviewReminderHtml(string candidateName, string jobTitle,
            DateTime scheduledAt, string meetingLink, string notes) 
        {
            var meetingLinkHtml = !string.IsNullOrWhiteSpace(meetingLink) 
                ? $"<p> <a href=\"{meetingLink}\">{meetingLink}</a></p>" 
                : "";
                
            var notesHtml = !string.IsNullOrWhiteSpace(notes)
                ? $"<p><strong>Message from the Recruiter:</strong><br/>{notes.Replace("\n", "<br/>")}</p>"
                : "";

            return $@"
            <p>Hi {candidateName},</p>
            <p>This is a reminder that your interview for the <strong>{jobTitle}</strong> position
            is scheduled for <strong>{scheduledAt:f}</strong>.</p>
            {meetingLinkHtml}
            {notesHtml}
            <p>Good luck!</p>
            <p>&mdash; Recruitment Team</p>";
        }

        /// <summary>Generates the subject line for an application status update email.</summary>
        /// <param name="jobTitle">The title of the job position.</param>
        /// <returns>The formatted email subject.</returns>
        public static string StatusUpdateSubject(string jobTitle) =>
            $"Application Update: {jobTitle}";

        /// <summary>Generates the HTML body for an application status update email.</summary>
        /// <param name="candidateName">The candidate's full name.</param>
        /// <param name="jobTitle">The title of the job position.</param>
        /// <param name="newStatus">The new application status.</param>
        /// <returns>The formatted HTML email body.</returns>
        public static string StatusUpdateHtml(string candidateName, string jobTitle, string newStatus) => $@"
            <p>Hi {candidateName},</p>
            <p>Your application status for the <strong>{jobTitle}</strong> position has been updated to:
            <strong>{newStatus}</strong>.</p>
            <p>You can check the details from your candidate dashboard.</p>
            <p>&mdash; Recruitment Team</p>";
    }
}
