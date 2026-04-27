using Raktarkezelo.Data;
using Raktarkezelo.Models.Entities;

namespace Raktarkezelo.Services
{
    public class AuditService
    {
        private readonly RaktarDb _context;

        public AuditService(RaktarDb context)
        {
            _context = context;
        }

        /// <summary>
        /// Naplóbejegyzést ír az AuditLogs táblába.
        /// Try/catch védi: ha a tábla nem létezik (pl. migráció még nem futott),
        /// a naplózási hiba nem akadályozza meg a fő műveletet.
        /// </summary>
        public async Task LogAsync(int? userId, string username, string action, string? details = null)
        {
            try
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId    = userId,
                    Username  = username,
                    Action    = action,
                    Details   = details,
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch
            {
                // A naplózási hiba soha nem töri meg a fő műveletet.
            }
        }
    }
}
