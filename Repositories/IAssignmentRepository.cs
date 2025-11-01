using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;



        /// <summary>
        /// Lấy danh sách assignment quá hạn của student
        /// </summary>
        Task<List<Assignment>> GetOverdueAssignmentsAsync(Guid studentId);
    }
}
