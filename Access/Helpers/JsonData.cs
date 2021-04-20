using System;
using System.Collections.Generic;
using DORA.Access.Models;
using Newtonsoft.Json;

namespace DORA.Access.Helpers
{
	public class JsonError
	{
		public int code { get; set; }
		public string message { get; set; }

		public JsonError(string message, int code = 0)
		{
			this.message = message;
			this.code = code;
		}
	}

	public class JsonData<TEntity>
	{
		public string message { get; set; }
		public bool success { get; set; }
		public string access_token { get; set; }
		public string refresh_token { get; set; }
		public PagedMetaData page_meta { get; set; }
		public List<TEntity> records { get; set; }
		public string recordType { get; set; }
		public List<JsonError> errors { get; set; }

		public JsonData(
			List<TEntity> records,
			string access_token,
			string refresh_token,
			PagedMetaData page_meta = null,
			bool success = true,
			string message = null)
		{
			this.records = records;
			this.recordType = records != null && records.Count > 0 ? records[0].GetType().ToString() : null;
			this.access_token = access_token;
			this.refresh_token = refresh_token;
			this.page_meta = page_meta;
			this.success = success;
			this.message = message;
		}

		public JsonData(
			List<TEntity> records,
			PagedMetaData page_meta = null,
			bool success = true,
			string message = null)
		{
			this.records = records;
			this.recordType = records != null && records.Count > 0 ? records[0].GetType().ToString() : null;
			this.page_meta = page_meta;
			this.success = success;
			this.message = message;
		}

		public JsonData(
			TEntity record,
			string access_token,
			string refresh_token,
			bool success = true,
			string message = null)
		{
			this.records = new List<TEntity>() { record };
			this.recordType = record != null ? record.GetType().ToString() : null;
			this.access_token = access_token;
			this.refresh_token = refresh_token;
			this.success = success;
			this.message = message;
		}

		public JsonData(
			TEntity record,
			bool success = true,
			string message = null)
		{
			this.records = new List<TEntity>() { record };
			this.recordType = record != null ? record.GetType().ToString() : null;
			this.success = success;
			this.message = message;
		}

		public string Serialize(int maxDepth = 1)
		{
			JsonSerializerSettings options = new JsonSerializerSettings
			{
				MaxDepth = maxDepth,
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			};
			
			if (this.page_meta != null)
			{
				return JsonConvert.SerializeObject(new
				{
					data = this.records,
                    type = this.recordType,
                    page_meta = this.page_meta,
                    success = this.success,
                    message = this.message,
                    access_token = this.access_token,
                    refresh_token = refresh_token,
                    api_errors = this.errors,
                }, options);
			}
			return JsonConvert.SerializeObject(new
			{
				data = this.records,
				type = this.recordType,
				success = this.success,
				message = this.message,
				access_token = this.access_token,
				refresh_token = refresh_token,
				api_errors = this.errors,
			}, options);
		}

		public bool AddError(JsonError error)
		{
			if (this.errors == null)
				this.errors = new List<JsonError>();

			int countBefore = this.errors.Count;
			this.errors.Add(error);

			return this.errors.Count > countBefore;
		}

		public bool AddError(string message, int code = 0)
		{
			if (this.errors == null)
				this.errors = new List<JsonError>();

			int countBefore = this.errors.Count;
			JsonError error = new JsonError(message, code);
			this.errors.Add(error);

			return this.errors.Count > countBefore;
		}
	}

	public class JsonDataError : JsonData<JsonError>
	{
		public JsonDataError(
			List<JsonError> errors,
			string message = "Error",
			string access_token = null,
			string refresh_token = null
		) : base(errors, access_token, refresh_token, null, false, message)
		{
			this.errors = errors;
		}

		public JsonDataError(
			string errorMessage,
			int errorCode = 0,
			string message = "Error",
			string access_token = null,
			string refresh_token = null
		) : base(null, access_token, refresh_token, false, message)
		{
			this.AddError(errorMessage, errorCode);
		}
	}
}
