/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the LoginProtocol library.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s):
*/

using System;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.Caching;
using System.IO;
using Parlo;

namespace LoginProtocol.Database
{
    /// <summary>
    /// Represents a cache for the database, used to retrieve users and cache them.
    /// </summary>
    public class UserCache : IDisposable
    {
        private MemoryCache m_Cache;
        private readonly string m_CacheFilePath;
        private readonly TimeSpan m_CacheItemSlidingExpiration;
        private readonly long m_CacheSizeLimitInBytes;

        /// <summary>
        /// Creates a new instance of UserCache.
        /// </summary>
        /// <param name="CacheFilePath">The path at which to store the cache on disk.</param>
        /// <param name="CacheSizeLimitInBytes">The size limit of the cache, in bytes.</param>
        public UserCache(string CacheFilePath, long CacheSizeLimitInBytes, TimeSpan SlidingExpiration)
        {
            m_CacheFilePath = CacheFilePath;

            // Initialize the cache with the specified memory limit and sliding expiration
            m_Cache = new MemoryCache("UserCache", new NameValueCollection
            {
                { "cacheMemoryLimitMegabytes", (CacheSizeLimitInBytes / (1024 * 1024)).ToString() }
            });

            m_CacheItemSlidingExpiration = SlidingExpiration;

            //Load cached users from file (if exists)
            LoadCacheFromFile();
        }

        /// <summary>
        /// Retrieves a user from the DB.
        /// </summary>
        /// <param name="Username">The name of the user to retrieve.</param>
        /// <returns>A new user if it was found, or am empty (not null) instance of User if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if Username is null.</exception>
        /// <exception cref="ArgumentException">Thrown if Username is empty.</exception>
        public User GetUser(string Username)
        {
            if (Username == null)
                throw new ArgumentException("Username");
            if (Username == string.Empty)
                throw new ArgumentException("Username cannot be empty!");

            //Try to get user from cache
            User? user = m_Cache.Get(Username) as User;
            if (user != null)
                return user;

            //User not found in cache, try to get from database
            DataTable Result = Database.SelectFrom("Users", "Username", Username);

            if (Result.Rows.Count > 0)
            {
                DataRow Row = Result.Rows[0];
                User U = new User
                {
                    Username = (string)Row["Username"],
                    Salt = (string)Row["Salt"],
                    Verifier = (string)Row["Verifier"]
                };

                m_Cache[Username] = U;

                SaveCacheToFile();

                return U;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a user from the cache, without querying the database.
        /// </summary>
        /// <param name="Username">The name of the user.</param>
        /// <returns> A user instance.</returns>
        public User GetUserFromCache(string Username)
        {
            User? user = m_Cache.Get(Username) as User;
            return user;
        }

        /// <summary>
        /// Adds a user to the DB.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if User is null.</exception>
        public void AddUser(User User)
        {
            if (User == null)
                throw new ArgumentException("Username");

            // Check if the user already exists in the database
            DataTable result = Database.SelectFrom("Users", "Username", User.Username);

            // If the user doesn't exist, insert them into the database
            if (result.Rows.Count == 0)
            {
                Database.InsertInto("Users", new string[] { "Username", "Salt", "Verifier" },
                    new string[] { User.Username, User.Salt, User.Verifier });
            }

            CacheItemPolicy Policy = new CacheItemPolicy
            {
                SlidingExpiration = m_CacheItemSlidingExpiration
            };

            m_Cache.Add(User.Username, User, Policy);

            SaveCacheToFile();
        }

        /// <summary>
        /// Removes a user from the DB.
        /// </summary>
        /// <param name="Username">The Username of the user to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if Username is null.</exception>
        /// <exception cref="ArgumentException">Thrown if Username is empty.</exception>
        public void RemoveUser(string Username)
        {
            if (Username == null)
                throw new ArgumentException("Username");
            if (Username == string.Empty)
                throw new ArgumentException("Username cannot be empty!");

            m_Cache.Remove(Username);

            SaveCacheToFile();
        }

        public void InvalidateCache()
        {
            m_Cache.Dispose();
            m_Cache = new MemoryCache("UserCache", new NameValueCollection
            {
                { "cacheMemoryLimitMegabytes", (m_CacheSizeLimitInBytes / (1024 * 1024)).ToString() }
            });

            if (File.Exists(m_CacheFilePath))
                File.Delete(m_CacheFilePath);
        }

        private void SaveCacheToFile()
        {
            // Save cache to file
            using (var Writer = new BinaryWriter(File.Open(m_CacheFilePath, FileMode.Create, FileAccess.ReadWrite,
                FileShare.ReadWrite)))
            {
                Writer.Write(m_Cache.GetCount());

                foreach (var user in m_Cache)
                {
                    Writer.Write(((User)user.Value).Username);
                    Writer.Write(((User)user.Value).Salt);
                    Writer.Write(((User)user.Value).Verifier);
                }
            }
        }

        private void LoadCacheFromFile()
        {
            if (File.Exists(m_CacheFilePath))
            {
                using (BinaryReader Reader = new BinaryReader(File.Open(m_CacheFilePath, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.ReadWrite)))
                {
                    int Count = Reader.ReadInt32();

                    for (int i = 0; i < Count; i++)
                    {
                        var user = new User
                        {
                            Username = Reader.ReadString(),
                            Salt = Reader.ReadString(),
                            Verifier = Reader.ReadString()
                        };

                        m_Cache.Set(user.Username, user, new CacheItemPolicy { SlidingExpiration = m_CacheItemSlidingExpiration });
                    }
                }
            }
        }

        ~UserCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the resources used by this UserCache instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of the resources used by this UserCache instance.
        /// <param name="Disposed">Was this resource disposed explicitly?</param>
        /// </summary>
        protected virtual void Dispose(bool Disposed)
        {
            if (Disposed)
            {
                if (m_Cache != null)
                    m_Cache.Dispose();

                // Prevent the finalizer from calling ~UserCache, since the object is already disposed at this point.
                GC.SuppressFinalize(this);
            }
            else
                Logger.Log("UserCache not explicitly disposed!", LogLevel.error);
        }
    }
}