namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public class AccountModel 
    {
        private string _accid;
        /// <summary>
        ///  帐套ID
        /// </summary>
        public string Accid
        {
            get { return _accid; }
            set
            {
                _accid = value;
              
            }
        }

        private string _accname;
        /// <summary>
        ///  帐套名称
        /// </summary>
        public string Accname
        {
            get { return _accname; }
            set
            {
                _accname = value;
               
            }
        }

        private int _acctype;
        private bool _isSelected;

        /// <summary>
        ///  帐套类型
        /// </summary>
        public int Acctype 
        {
            get { return _acctype; }
            set
            {
                _acctype = value;
               
            }
        }

       
    }
}
