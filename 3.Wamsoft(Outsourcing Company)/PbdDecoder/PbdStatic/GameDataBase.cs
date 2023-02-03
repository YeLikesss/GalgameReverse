using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PbdStatic.Database
{


    /// <summary>
    /// 花鐘カナデ グラム Chapter:1 小桜結
    /// <para>花鐘カナデ グラム Chapter:2 花ノ香澄玲</para>
    /// <para></para>
    /// <para></para>
    /// </summary>
    internal class HanaganeKanadeGram : GameInformationBase
    {
    }

    /// <summary>
    /// 七ヶ音学園 -旅行部-
    /// <para>七ヶ音学園 -旅行部- Tour: 02 Hakone編</para>
    /// </summary>
    internal class NanaganeGakuenTravel : GameInformationBase
    {
    }

    /// <summary>
    /// AMBITIOUS MISSION
    /// </summary>
    internal class AmbitiousMission : GameInformationBase
    {
    }

    /// <summary>
    /// ツヴァイトリガー
    /// </summary>
    internal class ZweiTrigger : GameInformationBase
    {
    }

    /// <summary>
    /// キスからはじめるエゴイズム
    /// </summary>
    internal class EgoismStartingWithaKiss : GameInformationBase
    {
    }

    /// <summary>
    /// RiddleJoker
    /// </summary>
    internal class RiddleJoker : GameInformationBase
    {
    }

    /// <summary>
    /// ATRI -My Dear Moments-
    /// </summary>
    internal class AtriMyDearMoment : GameInformationBase
    {
    }

    public class DataManager
    {
        private static Dictionary<string, GameInformationBase> mSDatabase = new(32)
        {
            { "Hanagane Kanade Gram Chapter:1 Kozakura Yui", new HanaganeKanadeGram() },
            { "Hanagane Kanade Gram Chapter:2 Hananoka Sumire", new HanaganeKanadeGram() },
            { "Nanagane Gakuen Ryokou Bu", new NanaganeGakuenTravel() },
            { "Nanagane Gakuen Ryokou Bu Tour:02 Hakone Hen", new NanaganeGakuenTravel() },
            { "Ambitious Mission", new AmbitiousMission() },
            { "Zwei Trigger", new ZweiTrigger() },
            { "Egoism Starting With a Kiss", new EgoismStartingWithaKiss() },
            { "Riddle Joker", new RiddleJoker() },
            { "ATRI -My Dear Moments-", new AtriMyDearMoment() },
        };

        /// <summary>
        /// 获取数据库
        /// </summary>
        public static Dictionary<string, GameInformationBase> SDataBase
        {
            get
            {
                return mSDatabase;
            }
        }
    }

}
