namespace NetworkAdapter.Entities
{
    public class UserExtraInfo
    {
        public int AvatarIndex { get; set; }
        public string DisplayName { get; set; }
    }

	public class UserNameChangeResult
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public string OldName { get; set; }
		public string NewName { get; set; }
	}

}