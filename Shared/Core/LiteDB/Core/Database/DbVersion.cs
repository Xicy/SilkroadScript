using System;

namespace LiteDB
{
    public partial class LiteDatabase : IDisposable
    {
        /// <summary>
        ///     Virtual method for update database when a new version (from coneection string) was setted
        /// </summary>
        protected virtual void OnVersionUpdate(int newVersion)
        {
        }

        /// <summary>
        ///     Loop in all registered versions and apply all that needs. Update dbversion
        /// </summary>
        private void UpdateDbVersion(ushort recent)
        {
            var dbparams = _engine.Value.GetDbParam();
            Version = dbparams.DbVersion;

            for (var newVersion = Version + 1; newVersion <= recent; newVersion++)
            {
                Log.Write(Logger.COMMAND, "update database version to {0}", newVersion);

                OnVersionUpdate(newVersion);

                Version = dbparams.DbVersion = (ushort) newVersion;
                _engine.Value.SetParam(dbparams);
            }
        }
    }
}