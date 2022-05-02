using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace TeamAlpha.Source
{
    public partial class ProcessorSaveLoad
    {
        private delegate bool MigrateStage();
        List<MigrateStage> migrationList;
        private void SetupMigration()
        {
            migrationList = new List<MigrateStage>();

            migrationList.Add(MigrateTo1);
        }
        private void TryMigrate()
        {
            this.Log("Start Migration(Updating save file version).");
            int curVersion = Load(tagVersion, 0);
            this.Log("Current save file version: " + curVersion + "\n"
                + "Last save file version: " + lastVersion);
            //Launch migration chain
            for (int i = curVersion; i < migrationList.Count; i++)
            {
                if (migrationList[i].Invoke())
                {
                    this.Log("Migration to version " + (i + 1) + " was successful!");
                    //Update version number of file
                    Save(tagVersion, i + 1);
                }
                else
                    this.Log("Migration to version " + (i + 1) + " was NOT successful!");
            }


            //SaveAll();
            //LoadGameData();
            OnLocalDataUpdated.Invoke();
        }
        private bool MigrateTo1()
        {
            return true;
        }
    }
}
