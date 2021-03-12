using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace crecheng.DSPModSave
{
    public interface IModCanSave
    {

        /// <summary>
        /// 保存数据
        /// 存档成功后调用
        /// save data
        /// </summary>
        /// <param name="w"></param>
        void Export(BinaryWriter w);

        /// <summary>
        /// 读取数据
        /// 读档成功后调用
        /// read data
        /// </summary>
        /// <param name="r"></param>
        void Import(BinaryReader r);

        /// <summary>
        /// 没有mod存档但进了新存档
        /// no mod save ,but into other save
        /// </summary>
        void IntoOtherSave();
    }
}
