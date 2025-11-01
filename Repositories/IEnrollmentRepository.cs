using System;
using BTL_QuanLyLopHocTrucTuyen.Core.Repositories;
using BTL_QuanLyLopHocTrucTuyen.Models;



        /// <summary>
        /// Lấy enrollment theo userId và courseId
        /// </summary>
        Task<Enrollment?> GetEnrollmentByUserAndCourseAsync(Guid userId, Guid courseId);
    }
}
