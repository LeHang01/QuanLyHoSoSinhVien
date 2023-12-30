using Model.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DAO
{
    public class SVDao
    {
        CamTraiDbContext db = null;
        public SVDao()
        {
            db = new CamTraiDbContext();
        }

        public List<tb_SinhVien> ListAll()
        {
            return db.tb_SinhVien.Where(x => x.Status == true).OrderBy(x => x. CreatedDate).ToList();
        }

        
    }
}
