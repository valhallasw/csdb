using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace CodeRun
{

	public static class TraceHelper
	{
		public static void WriteLineEx(WebLogEntryType entryType, string category, string eventCode, string message, string data, string identity)
		{
			WriteLineEx(entryType.ToString(), category, eventCode, message, data, identity);
		}
		public static void WriteLineEx(string entryType, string category, string eventCode, string message, string data, string identity)
		{
			string[] parts = new string[7];
			parts[0] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
			parts[1] = entryType.IsNullOrEmpty() ? " " : entryType; //entryType ?? " ";
			parts[2] = category.IsNullOrEmpty() ? " " : category; //category ?? " ";
			parts[3] = eventCode.IsNullOrEmpty() ? " " : eventCode; //eventCode ?? " ";
			parts[4] = message.IsNullOrEmpty() ? " " : message;
			parts[5] = data.IsNullOrEmpty() ? " " : data; //data ?? " ";
			parts[6] = identity.IsNullOrEmpty() ? " " : identity;
			Trace.WriteLine(parts.StringConcat(((char)0).ToString()));
		}
	}

	// Summary:
	//     Specifies the event type of an event log entry.
	public enum WebLogEntryType
	{
		// Summary:
		//     An error event. This indicates a significant problem the user should know
		//     about; usually a loss of functionality or data.
		Error,
		//
		// Summary:
		//     A warning event. This indicates a problem that is not immediately significant,
		//     but that may signify conditions that could cause future problems.
		Warning,
		//
		// Summary:
		//     An information event. This indicates a significant, successful operation.
		Information,
		//
		// Summary:
		//     A success audit event. This indicates a security event that occurs when an
		//     audited access attempt is successful; for example, logging on successfully.
		SuccessAudit,
		//
		// Summary:
		//     A failure audit event. This indicates a security event that occurs when an
		//     audited access attempt fails; for example, a failed attempt to open a file.
		FailureAudit,
	}
}