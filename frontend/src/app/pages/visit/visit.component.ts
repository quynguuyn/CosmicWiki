import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

interface ObservatoryEvent {
  date: string;
  name: string;
  description: string;
  specification: string;
  location: string;
  status: string;
  statusType: 'primary' | 'tertiary' | 'secondary' | 'outline';
  action: string;
}

interface Exhibition {
  number: string;
  category: string;
  title: string;
  description: string;
  status: string;
  image: string;
}

interface FaqItem {
  question: string;
  answer: string;
}

@Component({
  selector: 'app-visit',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  templateUrl: './visit.component.html',
  styleUrl: './visit.component.scss'
})
export class VisitComponent {

  constructor(private readonly api: ApiService) {}

  currentYear = new Date().getFullYear();

  isSubmitting = false;
  showConfirmationModal = false;
  submissionError = '';
  referenceCode = '';

  form = {
    name: '',
    email: '',
    subject: 'booking',
    message: ''
  };

  exhibitions: Exhibition[] = [
    {
      number: '01',
      category: 'Hệ Hành Tinh & Quỹ Đạo',
      title: 'Đại Sảnh Thái Dương Hệ',
      description:
        'Mô hình quỹ đạo thực tế 8 hành tinh, tương tác phóng to vành đai Sao Thổ và chi tiết địa hình các miệng hố Mặt Trăng bằng dữ liệu vi thám laser.',
      status: 'Tương tác 3D',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuAydwL1FfZiCuewaFf7utYfwbOZ_QixFJrFSVhn6RnBuPsJq6xJr0ykKgeV6JUPH5TywhW1MzexeDkkfEuB3qStV2o-jYTHHRh51b1mQmJpyvl5rOLdJ1J_mp9BtS-ShK6alVRBmK8L3_GF2jkl_mQGbq2eMDLIjFxs_kyCI6twwVI5BZ6dIusuyPKJHrme6UTYoVSPceYviO3DMKg-jCiRmtEZieVz_5Lx4oo54qLeQi5oM4LeCZFu3pH9ot93dKXU9DGHcAmFFA'
    },
    {
      number: '02',
      category: 'Thế Kỷ XVI - XIX',
      title: 'Vòm Sao & Cổ Thiên Văn',
      description:
        'Chiêm ngưỡng Thước trắc tinh Astrolabe, Bản đồ sao Dürer 1515, và các dụng cụ định vị cổ đại bằng đồng thau từng đồng hành cùng các hải trình vượt đại dương.',
      status: 'Di sản quý',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuA2u4zlh_W0_l85sCz8zV9eB8-xL2ODrt0unfLyx7OYNYrOWn2FN1TBi-Oi1_sTOMJ3RbpI42PKlGkl3s83S8sxPANw-vnnDWWzYBt17C4E04rT88qsCxmE6O4qDQygyxg9ogcpFmjPqVCh_Bn2lKCWkzwZL5gq0HryCtI-RPy9Et8MQphM7l0FTj61n6FauAFD7S_x2-8WZXPXiD4MGy7Y_hTTWYx3EL7G337uPpDbhwA4gnAtRg2vJTySgLPuf92RkHsVtXBubg'
    },
    {
      number: '03',
      category: 'Vũ Trụ Sâu & Quang Phổ',
      title: 'Vườn Ươm Thiên Hà & Hố Đen',
      description:
        'Dữ liệu hồng ngoại từ M16 Cột Sáng Sáng Tạo, Hố đen M87* và Vùng nhiễu xạ James Webb tái tạo quang phổ nguyên tố sơ khai của vũ trụ.',
      status: 'Dữ liệu JWST',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuDouQMCeBOwdUrwJ6Si4zXeEDya-VEcqz9nnRR_Z9i6TNjsMNvN_WXuBaBJSa0GhZNdlDSJO1Az_00ShOjw4ko__EcW5TONr-lUnv76V5YsAEI41x00UHlYK49MQntHRT07Pax6P9YldnTIoZ0j0s9RQUXI71dsf_9xx2UpAcCi9DcZ-F24Gx3nNU5h4cytPGznfGJpg7ntjPd0NCpY3yLcL0nb0diZGVMRq-XT_fcjb_8vDD7THr0r_Xi5nU0FxenFCfez1OQt4w'
    },
    {
      number: '04',
      category: 'Thám Hiểm Biên Giới',
      title: 'Kỷ Nguyên Viễn Chinh Không Gian',
      description:
        'Di sản Voyager 1, đĩa vàng mạ kim mang thông điệp loài người, trạm Cassini và hành trình khai phá biên giới nhật quyển của nền văn minh Trái Đất.',
      status: 'Tư liệu gốc',
      image:
        'https://lh3.googleusercontent.com/aida-public/AB6AXuA79oD_3zLwvNImxVKx-jmW3YmWMkorJ-2V2kWMXmYOBlXDCncMoW_7vy__hz1xHrEwmRzfHjpo3GACKqyDXXrsEGIEAF-cUPVzWVDvB6XUavM2aulrI-3Fs7CdU4z7II2okFBNeqV91XyA2U2rZ04O5jMHdbgzlwueJIX8MsB1jZk48uGrZ1lhlBvvflXH0X8Bv2ZVE3SnFFlt4Pm-l896UGSScpsrwOZsAdiJVzsc-PVKiOg4yK7dzGbBGw0amiq6QRK7xBFHyg'
    }
  ];

  events: ObservatoryEvent[] = [
    {
      date: '12 - 13 Tháng 8, 2025',
      name: 'Mưa sao băng Perseids',
      description: 'Tàn dư sao chổi 109P/Swift-Tuttle',
      specification: 'Cực đại 100 vệt/giờ',
      location: 'Góc phương vị 45° Đông Bắc',
      status: 'Mở đăng ký luồng livestream',
      statusType: 'primary',
      action: 'Đăng Ký Luồng'
    },
    {
      date: '21 Tháng 9, 2025',
      name: 'Giao hội Sao Thổ & Mặt Trăng',
      description: 'Góc ly giác cực tiểu 0.3°',
      specification: 'Quan sát rõ vành đai Cassini',
      location: 'Kính phản xạ phức hợp 10.4m',
      status: 'Phiên phân tích quang phổ mở',
      statusType: 'tertiary',
      action: 'Nhận Tọa Độ'
    },
    {
      date: '14 Tháng 10, 2025',
      name: 'Nhật thực hình khuyên',
      description: 'Đường đi quét qua bán cầu tây',
      specification: 'Bộ lọc Hydro-Alpha (Hα)',
      location: 'Ghi nhận bão sắc cầu Mặt Trời',
      status: 'Phiên ghi nhận dữ liệu FITS',
      statusType: 'secondary',
      action: 'Tải Mẫu FITS'
    },
    {
      date: '18 Tháng 11, 2025',
      name: 'Mưa sao băng Leonids',
      description: 'Cụm tàn dư sao chổi Tempel-Tuttle',
      specification: 'Vận tốc hạt 71 km/giây',
      location: 'Chòm sao Sư Tử (Leo)',
      status: 'Chuẩn bị thiết bị',
      statusType: 'outline',
      action: 'Nhắc Nhở Lịch'
    }
  ];

  faqs: FaqItem[] = [
    {
      question: 'Làm thế nào để truy cập tệp dữ liệu FITS thô từ đài quan sát?',
      answer:
        'Tất cả các bản chụp thiên văn sau khi qua bộ tiền xử lý khử nhiễu nhiệt (Dark frame) và hiệu chỉnh trường phẳng (Flat frame) được lưu trữ tại máy chủ lưu trữ FITS mở của CosmicWiki. Các nhà nghiên cứu có thể dùng mã định danh IAU hoặc tải qua cổng giao tiếp API Python Astropy hoàn toàn miễn phí.'
    },
    {
      question: 'Lịch đặt giờ kính thiên văn dành cho các nhà nghiên cứu độc lập hoạt động ra sao?',
      answer:
        'Thời lượng quan sát của kính 10.4m được chia theo các chu kỳ hàng quý. Đề xuất quan sát (Observation Proposal) cần nêu rõ tọa độ thiên thể, góc nâng, thời gian phơi sáng yêu cầu và các bộ lọc quang phổ cần kích hoạt. Hội đồng giám định sẽ duyệt trong vòng 5 ngày làm việc.'
    },
    {
      question: 'Có thể tham quan đài thiên văn trực tiếp tại thực địa Mauna Kea / La Silla không?',
      answer:
        'Có. Chúng tôi tổ chức các đoàn tham quan học thuật thực địa vào các tuần trăng non (New Moon) hàng tháng nhằm giảm thiểu tối đa ô nhiễm ánh sáng. Yêu cầu đăng ký trước tối thiểu 30 ngày để sắp xếp thủ tục y tế thích nghi độ cao trên 2.800m.'
    },
    {
      question: 'Định dạng bản quyền hình ảnh vũ trụ và hiện vật số được quy định như thế nào?',
      answer:
        'Toàn bộ tư liệu ảnh thiên văn sâu, mô hình quét 3D thước trắc tinh và bản đồ sao cổ tại CosmicWiki được phát hành theo giấy phép Creative Commons Attribution 4.0 Quốc tế (CC BY 4.0), cho phép sử dụng cho mục đích giáo dục, nghiên cứu và xuất bản phi thương mại.'
    }
  ];

  submitTransmission(): void {
    if (this.isSubmitting || !this.form.name.trim() || !this.form.email.trim() || !this.form.message.trim()) {
      return;
    }

    this.isSubmitting = true;
    this.submissionError = '';

    this.api.post<
      {
        name: string;
        email: string;
        requestedDateUtc: string;
        partySize: number;
        notes: string;
      },
      { referenceCode?: string }
    >('/api/visit/requests', {
      name: this.form.name,
      email: this.form.email,
      requestedDateUtc: new Date().toISOString(),
      partySize: 1,
      notes: `[${this.form.subject}] ${this.form.message}`
    }).subscribe({
      next: response => {
        this.referenceCode = response.referenceCode ?? '';
        this.showConfirmationModal = true;
        this.form = {
          name: '',
          email: '',
          subject: 'booking',
          message: ''
        };
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting = false;
        this.submissionError = this.getSubmissionError(error);
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }

  private getSubmissionError(error: HttpErrorResponse): string {
    if (error.status === 400) {
      const validationErrors = error.error?.errors;
      if (validationErrors) {
        const messages = Object.values(validationErrors).flatMap(value =>
          Array.isArray(value) ? value : [String(value)]
        );
        if (messages.length > 0) {
          return messages.join(' ');
        }
      }
      return 'Vui lòng kiểm tra lại thông tin đăng ký và thử lại.';
    }

    if (error.status === 0) {
      return 'Không thể kết nối với hệ thống đặt lịch. Vui lòng thử lại sau.';
    }

    return 'Tín hiệu chưa được ghi nhận. Vui lòng thử lại sau.';
  }

  closeModal(): void {
    this.showConfirmationModal = false;
  }

  scrollToBooking(): void {
    document
      .getElementById('booking-transmission')
      ?.scrollIntoView({ behavior: 'smooth' });
  }
}