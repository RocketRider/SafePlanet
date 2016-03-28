using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security.Cryptography;


namespace SafePlanet
{
    [Serializable()]
    public class PasswordElement : ISerializable
    {
        private PasswordManager pwMGR;
        public void setPasswordManager( PasswordManager pwMGR)
        {
            this.pwMGR = pwMGR;
        }


        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        private string url;
        public string URL
        {
            get { return url; }
            set { url = value; }
        }

        private string password;
        private int passwordlen;
        
        public string Password
        {
            get { return Utilities.starPassword(passwordlen); }
            set 
            {
                passwordlen = value.Length;
                password = Utilities.EncryptString(value, Utilities.GetSHA512Hash(pwMGR.MasterPassword), Salt);
                updateChangeDate();
            }
        }
        public string getPassword()
        {
            string res="";
            if (password != "")
            {
                res = Utilities.DecryptString(password, Utilities.GetSHA512Hash(pwMGR.MasterPassword), Salt); 
            }

            return res;
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }



        private byte[] salt;
        public byte[] Salt
        {
            get { return salt; }
            set { salt = value; }
        }

        private string id;
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        private string group;
        public string Group
        {
            get { return group; }
            set { group = value; }
        }

        private long changeDate; //(long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds
        public long ChangeDate
        {
            get { return changeDate; }
            set { changeDate = value; }
        }
        public void updateChangeDate()
        {
            changeDate = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }


        public PasswordElement(SerializationInfo info, StreamingContext ctxt)
        {
            this.username = (string)info.GetValue("username", typeof(string));
            this.url = (string)info.GetValue("url", typeof(string));
            this.password = (string)info.GetValue("password", typeof(string));
            this.description = (string)info.GetValue("description", typeof(string));
            this.passwordlen = (int)info.GetValue("passwordlen", typeof(int));
            this.salt = (byte[])info.GetValue("salt", typeof(byte[]));
            this.id = (string)info.GetValue("id", typeof(string));
            this.group = (string)info.GetValue("group", typeof(string));
            this.changeDate = (long)info.GetValue("changeDate", typeof(long));
        }

        public PasswordElement(string username, string url, string password, string description, string id, long changeDate, PasswordManager mgr)
        {
            this.salt = Utilities.CreateRandomSalt(16);
            this.pwMGR = mgr;

            this.username = username;
            this.url = url;
            this.Password = password;
            this.description = description;
            //this.passwordlen = passwordlen;

            if (id == null || id.Length < 1)
            {
                this.id = Utilities.CreateRandomID();
            }else
            {
                this.id = id;
            }
            if (changeDate < 1)
            {
                updateChangeDate();
            }else
            {
                this.ChangeDate = changeDate;
            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("username", this.username);
            info.AddValue("url", this.url);
            info.AddValue("password", this.password);
            info.AddValue("description", this.description);
            info.AddValue("passwordlen", this.passwordlen);
            info.AddValue("salt", this.salt);
            info.AddValue("id", this.id);
            info.AddValue("changeDate", this.changeDate);
            info.AddValue("group", this.group);
        }



    }


    public class PasswordManager
    {

        public static byte[] FromFile_GetID(string file)
        {
            byte[] id = new byte[16];
            FileStream stream = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                if (File.Exists(file))
                {

                    stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
                    stream.Read(id, 0, 16);
                }
            }

            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return id;
        }

        public static string getSecureKey_FromID(byte[] id)
        {
            string machineID = Utilities.getMachineID();
            string secPW = Utilities.ReadRegistrySecure(System.BitConverter.ToString(id), machineID, id);
            return secPW;
        }

        public static void setSecureKey_ForID(byte[] id, string secPW)
        {
            string machineID = Utilities.getMachineID();
            Utilities.WriteRegistrySecure(System.BitConverter.ToString(id), secPW, machineID, id);
        }


        public static void importSecureKey(string file, string encpw)
        {
            FileStream stream = null;
            try
            {
                //IFormatter formatter = new BinaryFormatter();
                if (File.Exists(file))
                {

                    stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);

                    byte[] id = new byte[16];
                    stream.Read(id, 0, 16);

                    byte[] salt = new byte[16];
                    stream.Read(salt, 0, 16);

                    byte[] data = new byte[stream.Length - 32];
                    stream.Read(data, 0, (int)stream.Length - 32);
                    string secKey;
                    if (encpw.Length >0)
                        secKey = (string)Utilities.ByteArrayToObject(Utilities.DecryptBytes(data,  encpw, salt));
                    else
                        secKey = (string)Utilities.ByteArrayToObject(data);

                    setSecureKey_ForID(id, secKey);

                }
            }

            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }


        }



        public static void syncFiles(string[] files, string password)
        {
            PasswordManager mgr = new PasswordManager();
            mgr.pwfile = files[0];
            mgr.MasterPassword = Utilities.GetSHA256Hash(password);
            mgr.loadPasswordlist();

            long syncDate = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;

            for(int i = 1;i<files.Length;i++)
            {
                PasswordManager tempMgr = new PasswordManager();
                tempMgr.pwfile = files[i];
                tempMgr.MasterPassword = Utilities.GetSHA256Hash(password);
                tempMgr.loadPasswordlist();

                foreach (PasswordElement tempel in tempMgr.passwordList)
                {
                    if (tempel.ChangeDate > syncDate)
                    {
                        System.Windows.Forms.MessageBox.Show("The element " + tempel.URL + " from " + files[i] + " seems to come from the future!");
                        return;
                    }
                    if (tempel.ChangeDate < 1)
                    {
                        System.Windows.Forms.MessageBox.Show("The element " + tempel.URL + " from " + files[i] + " does not have a correct timestamp!");
                        return;
                    }

                    bool found = false;
                    foreach (PasswordElement el in mgr.passwordList)
                    {
                        if (el.ChangeDate > syncDate)
                        {
                            System.Windows.Forms.MessageBox.Show("The element " + el.URL + " from " + files[0] + " seems to come from the future!");
                            return;
                        }
                        if (el.ChangeDate < 1)
                        {
                            System.Windows.Forms.MessageBox.Show("The element " + el.URL + " from " + files[0] + " does not have a correct timestamp!");
                            return;
                        }


                        if (tempel.ID == el.ID)
                        {
                            found = true;
                            if (tempel.ChangeDate > el.ChangeDate)//Element was changed!
                            {
                                el.Password = tempel.getPassword();
                                el.Group = tempel.Group;
                                el.Description = tempel.Description;
                                el.URL = tempel.URL;
                                el.Username = tempel.Username;
                                el.ChangeDate = tempel.ChangeDate;
                            }

                        }
                    }
                    if (found == false)//New Element
                    {
                        mgr.add(tempel.Username, tempel.URL, tempel.getPassword(), tempel.Description, tempel.ID, tempel.ChangeDate);
                    }

                }
                
            }

            mgr.pwfile = DateTime.Now.ToShortTimeString() + "." + DateTime.Now.Second + "_" + DateTime.Now.ToShortDateString() + ".safeplanet";
            mgr.pwfile = mgr.pwfile.Replace(":", ".");
            mgr.savePasswordlist();

            try { Directory.CreateDirectory("backup\\"); }
            catch { }
            foreach (string file in files)
            {
                try { System.IO.File.Delete("backup\\" +  System.IO.Path.GetFileName(file)); }
                catch { }
                try { System.IO.File.Move(System.IO.Path.GetFileName(file), "backup\\" + System.IO.Path.GetFileName(file)); }
                catch { }
            }



        }




        public List<PasswordElement> passwordList = new List<PasswordElement>();
        private Boolean saved=true;
        private string masterPassword = "";
        private byte[] id;
        public string pwfile = "";

        public byte[] getID()
        {
            return id;
        }


        public PasswordManager()
        {
            id = Utilities.CreateRandomSalt(16);
            //pwfile = "data.dat"; 
        }


        public string MasterPassword
        {
            get { return masterPassword; }
            set { masterPassword = value; }
        }

        

        public Boolean isSaved()
        {
            return saved;
        }

        public void changedData()
        {
            saved = false;
        }

        public void add(string username, string url, string password, string desc, string id, long changeDate)
        {
            PasswordElement el = new PasswordElement(username, url, password, desc, id, changeDate, this);
            //el.setPasswordManager(this);
            passwordList.Add(el);
            saved = false;
        }

        public void remove(PasswordElement el)
        {
            passwordList.Remove(el);
            saved = false;
        }

        public string getSecureKey()
        {
            return getSecureKey_FromID(id);
        }


        public bool exportSecureKey(string file, string encpw)
        {
            bool result = false;
            string seckey = getSecureKey();
            if (seckey.Length > 0)
            {
                FileStream stream = null;
                try
                {
                    stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);

                    stream.Write(id, 0, (int)id.Length);

                    if (encpw.Length > 0) // use pw:
                    {
                        byte[] salt = Utilities.CreateRandomSalt(16);
                        stream.Write(salt, 0, (int)salt.Length);

                        byte[] data = Utilities.EncryptBytes(Utilities.ObjectToByteArray(seckey), encpw, salt);
                        stream.Write(data, 0, (int)data.Length);
                    }
                    else//No pw:
                    {
                        byte[] salt = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        stream.Write(salt, 0, (int)salt.Length);

                        byte[] data = Utilities.ObjectToByteArray(seckey);
                        stream.Write(data, 0, (int)data.Length);
                    }

                    result = true;
                }
                catch { }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }

            return result;
        }


        public void loadPasswordlist()
        {
            FileStream stream = null;
            try
            {
                //IFormatter formatter = new BinaryFormatter();
                if (File.Exists(pwfile))
                {

                    stream = new FileStream(pwfile, FileMode.Open, FileAccess.Read, FileShare.None);

                    id = new byte[16];
                    stream.Read(id, 0, 16);
                    string secPW = getSecureKey();

                    byte[] salt = new byte[16];
                    stream.Read(salt, 0, 16);

                    byte[] data = new byte[stream.Length-32];
                    stream.Read(data, 0, (int)stream.Length-32);

                    passwordList = (List<PasswordElement>)Utilities.ByteArrayToObject(Utilities.DecryptBytes(data, MasterPassword + secPW, salt));

                    foreach (PasswordElement el in passwordList)
                    {
                        el.setPasswordManager(this);
                    }

                    

                }
            }
            
            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            



            saved = true;
        }

        public void savePasswordlist()
        {

            string secPW = getSecureKey();
            if (secPW == "")
            {
                int keyLen = 32;
                string tmp = Utilities.ReadRegistry("SecKeyLength");
                if (tmp != null && tmp.Length>0)
                {
                    keyLen = Convert.ToInt32(tmp);
                }

                secPW = Utilities.RandomString(keyLen);
                setSecureKey_ForID(id, secPW);
            }

            FileStream stream = null;
            try
            {
                //IFormatter formatter = new BinaryFormatter();
                //Directory.CreateDirectory("data\\");
                stream = new FileStream(pwfile+".part", FileMode.Create, FileAccess.Write, FileShare.None);

                stream.Write(id, 0, (int)id.Length);

                byte[] salt = Utilities.CreateRandomSalt(16);
                stream.Write(salt, 0, (int)salt.Length);

                byte[] data = Utilities.EncryptBytes(Utilities.ObjectToByteArray(passwordList), MasterPassword + secPW, salt);
                stream.Write(data, 0, (int)data.Length);
            }
            
            catch { }
            finally
            {
                if (stream != null)
                    stream.Close();

                try{Directory.CreateDirectory("backup\\");}catch {}
                try{System.IO.File.Delete("backup\\" + pwfile);}catch {}
                try{System.IO.File.Move(pwfile, "backup\\" + pwfile);}catch {}
                try{System.IO.File.Move(pwfile + ".part", pwfile);}catch {}
            }
             
            saved = true;
        }




    }

}




