using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Infrstructure.Cache
{
    /// <summary>
    /// 資料快取提供者的介面定義
    /// </summary>
    public interface IRedisCacheProvider
    {
        /// <summary>
        /// 建立或置換快取資料值
        /// </summary>
        /// <param name="key">鍵值，如果指定的鍵值不存在則新增一筆快取項目。</param>
        /// <param name="data">資料值</param>
        /// <remarks>資料值必須為可序列化的型別。</remarks>
        void Put(string key, object data);
        /// <summary>
        /// 建立或置換快取資料值
        /// </summary>
        /// <param name="key">鍵值，如果指定的鍵值不存在則新增一筆快取項目。</param>
        /// <param name="data">資料值</param>
        /// <param name="liveTime">存活時間</param>
        /// <remarks>資料值必須為可序列化的型別。</remarks>
        void Put(string key, object data, TimeSpan liveTime);
        /// <summary>
        /// 取得快取資料值
        /// </summary>
        /// <param name="key">鍵值</param>
        /// <returns>資料值</returns>
        object Get(string key);
        /// <summary>
        /// 移除指定的一個快取資料
        /// </summary>
        /// <param name="key">鍵值，如果鍵值不存在將被忽略。</param>
        void Remove(string key);
    }
}
