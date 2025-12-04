using Newtonsoft.Json;

// Chú ý: namespace phải khớp với tên Project của bạn
namespace TheBallStores.Helpers
{
    public static class SessionExtensions
    {
        // Hàm lưu Object vào Session (chuyển thành chuỗi JSON)
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // Hàm lấy Object từ Session (chuyển từ chuỗi JSON về Object)
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}