using AssemblyToProcess.Util;
using AssemblyToProcess.View;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace DotNetFlow.Fody
{
    static class GlobalExceptionDefinitions
    {

        #region Util

        // Exception Definition to AssemblyToProcess.View.JukeboxHelper.ToUpperLyric
        [ExceptionRaiseSite("rs01")]
        //[ExceptionChannel("EEC1", new Type[] { typeof(SqlException), typeof(DbException) })]
        //[ExceptionInterface(typeof(LibException))]
        public static void ToUpperLyric(this JukeboxHelper jukeboxHelper, string lyric) { }


        #endregion

        #region View

        // Exception Definition to AssemblyToProcess.View.JukeboxPlayer.Main
        [ExceptionRaiseSite("rs02")]
        static void Main(this JukeboxPlayer args) { }

        #endregion

        #region Handlers

        #endregion


    }
}