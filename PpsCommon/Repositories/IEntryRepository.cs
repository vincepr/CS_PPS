namespace PpsCommon.Repositories;

public interface IEntryRepository
{
   IEnumerable<Entry> GetEntries();
   Entry GetEntryById(string uuid);
   void InsertEntry(Entry entry);
   void DeleteEntry(string uuid);
   void UpdateEntry(Entry entry);
}

public class Entry
{
   public Guid Id;
   public Guid GroupId;

   public string? Name;
   public string? Password;
   public string? Url;
   public string? Notes;
   public DateTimeOffset Expires;
   public Dictionary<string, string>? CustomUserFields;
   public List<Tag>? Tags;

   public readonly DateTimeOffset Created;
   public readonly DateTimeOffset Modified;
   public readonly bool HasModifiyEntriesAccess;
   public readonly bool HasViewEntryContentsAccess;
   // public readonly SCommentPromt CommentPrompts;
}

public class Tag
{
   public string Name;
}

// Used by entries and folders to hold file attachments.
public class Attachment
{
   public Guid CredentialObjectId;
   public string FileName;
   public Byte[] FileData;
   public long FileSize;
}

public class EntryRepository : IEntryRepository
{
   public IEnumerable<Entry> GetEntries()
   {
      throw new NotImplementedException();
   }

   public Entry GetEntryById(string uuid)
   {
      throw new NotImplementedException();
   }

   public void InsertEntry(Entry entry)
   {
      throw new NotImplementedException();
   }

   public void DeleteEntry(string uuid)
   {
      throw new NotImplementedException();
   }

   public void UpdateEntry(Entry entry)
   {
      throw new NotImplementedException();
   }
}