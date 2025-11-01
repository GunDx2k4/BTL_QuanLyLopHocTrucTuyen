using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;



        /// <summary>
        /// Lấy điểm trung bình của student
        /// </summary>
        Task<double> GetAverageGradeAsync(Guid studentId);
    }
}
