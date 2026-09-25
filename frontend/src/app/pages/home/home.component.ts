import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

interface Chapter {
  id: number;
  emoji: string;
  label: string;
  chapterNum: string;
  chapterSub: string;
  title: string;
  description: string;
  meta: string;
  imageUrl: string;
  imageAlt: string;
  wide?: boolean;
  tags?: string[];
  date?: string;
  icons?: string[];
}

interface ArchiveItem {
  code: string;
  category: string;
  distance: string;
  title: string;
  description: string;
  classification: string;
  imageUrl: string;
  imageAlt: string;
}

interface StoreItem {
  badge: string;
  origin: string;
  title: string;
  description: string;
  price: string;
  imageUrl: string;
  imageAlt: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  chapters: Chapter[] = [
    {
      id: 1, emoji: '🌍', label: '0.000015 Năm ánh sáng', chapterNum: 'Chương 01', chapterSub: 'Khởi nguyên nội vi',
      title: 'Thái Dương Hệ (Solar System)', meta: '8 Hành tinh & 300+ Mặt trăng',
      description: 'Cái nôi sinh mệnh của loài người, vũ điệu quỹ đạo hoàn hảo của 8 hành tinh quanh ngôi sao Vàng G2V và vùng biên viễn vành đai Kuiper băng giá.',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAdHO61hgsfaeDarajhfZkWQv9H_cL2FGfNAaNwEHuYsW3cJIT7pT6JCLlPb-6VBheRg4qnTT4nukb2eDx291N6ShX7gTXFX6xO0HbQiSCm341MegChveB-zX3k6992h_ulNT5cHIC-N1rOT3o2WTCiHXgnRWRfo6bBZ4Gj55_ZtkbXXggCyNX2BXN9WJljPG4xYEEJAGqVMi0YnjNQ648cbGXYTsCBb0jxYnP_-5NZIfJ2iNUKTGyY7_D7L_rlpZaMqUufi-jNow',
      imageAlt: 'Solar System illustration'
    },
    {
      id: 2, emoji: '⭐', label: 'Lò phản ứng vũ trụ', chapterNum: 'Chương 02', chapterSub: 'Lõi nhiệt hạch',
      title: 'Những Vì Tinh Tú (Stars)', meta: 'Sơ đồ Hertzsprung-Russell',
      description: 'Vườn ươm sao trong tinh vân rực rỡ, phản ứng nhiệt hạch tổng hợp nên các nguyên tố của sự sống và những vụ nổ siêu tân tinh giải phóng năng lượng vô song.',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBKKgGj44h1IZ7iZ_8YABC7GIY1YNUN-fZHdUymUi3DM3mB_2dQf-FMqEXADeTtPV1FlUe1zhAF1dXvAndYlbG7fLJ_6Rd41UFajHkW4CkIS4rIreU-H7Ase3G5bU6OB-hEj6q2OY-7VX7IMTtczXOrBRYMruzCKMbaL8LrCPdOUBwVBIW1E0YUfeZ0mjcN8e9y6TZCfj3b-GouTF4t90sY8ePnVtPFM0m3H8_hLw0oIV7ZQLjP2SGMeh4K9SqxGcGrMHSjS0Mhvg',
      imageAlt: 'Stellar nursery nebula'
    },
    {
      id: 3, emoji: '🌀', label: 'Cấu trúc tỷ năm', chapterNum: 'Chương 03', chapterSub: 'Quần tụ vĩ mô',
      title: 'Các Thiên Hà (Galaxies)', meta: 'Nhóm Địa phương (Local Group)',
      description: 'Những ốc đảo xoắn ốc khổng lồ chứa hàng trăm tỷ tinh cầu, vũ điệu va chạm giữa Dải Ngân Hà và Tiên Nữ trong tương lai xa bốn tỷ năm.',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDDjIX5ggbIFBciW9txAzJGbEYEI1W2CYRHsrAzaGXywfMnwldoT-ZfeBC85ZrXrll4Img6WDMIcB2JOZS9tkZO7oXLsDNWMK8Hev6q0b_m9090wyxdAE29XtfMaHTjkRA75F8GYzG682p7MKSYR6f2iwjQYsKLCbJflGuzODXSbcwNdJwaNDxHGk_cECN3Np7t8NFSOHq6JnWxMqPi1gxGEv0OZMLD5G3gwEeXYWA9PUmcvvQj0pKU2D2mlAvv3vnqD1Omw2a_xQ',
      imageAlt: 'Spiral galaxy'
    },
    {
      id: 4, emoji: '🕳️', label: 'Bất biến hấp dẫn', chapterNum: 'Chương 04', chapterSub: 'Ranh giới hư vô',
      title: 'Hố Đen & Chân Trời Sự Kiện', meta: 'Bán kính Schwarzschild',
      description: 'Điểm kỳ dị nơi không gian và thời gian sụp đổ. Thấu kính hấp dẫn uốn cong ánh sáng và câu đố lớn nhất vẫn chưa có lời giải của cơ học lượng tử.',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCjiMjDIbyDLjpek5wBW1v2uDFm1gJwPAx7raHUccfDmLrwuhrOOVfIxxkdCvkX9mxajvqO5EsGqL0ag1VeQPs2gBZNrn73mZuW_r3583R-btzd0Qv7iSdcoO_yZa8G34MpJw-baAUDFecYncsnAGJyM2Zv5ewEtfetbZCM74RNiU4HmgPWlsBInK9qrNRptqEATCRAgLHNspZ5-IWEjFxz_IPBC6qsQFyxK30n-DqUaO4hdaqNkTplT4_4AS7swD9hNvEPDpNiVA',
      imageAlt: 'Black hole with accretion disk'
    },
    {
      id: 5, emoji: '🔭', label: 'Dấu chân nhân loại', chapterNum: 'Chương 05', chapterSub: 'Kỷ nguyên viễn chinh',
      title: 'Thám Hiểm Không Gian (Space Exploration)', meta: 'Voyager 1 • L2 • Webb Telescope',
      description: 'Tàu thám hiểm Voyager mang đĩa ghi vàng vượt ranh giới nhật quyển, kính viễn vọng không gian James Webb mở cánh cửa nhìn về bình minh của vũ trụ sơ khai, và những khát vọng thuộc địa hóa liên hành tinh của nền văn minh loài người.',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuB2j6UbXqDcUEFJLVPgYtEoDEj7Mww-4VzoEIMjF0CMhx8uuoiwcnWp_oQJskgJhqV5oaw8xjknW5vTNUcgyM7QSiePKe1jbzutKzGEdshVRhwTzeFi8xFLnSS70wtIKKOI65wNzx7m2E4g5hCD2txRHFtEVPw120SbIULP3-sFuAEN6Okl7r9orx91ug6twOc5OvpA4CtllxduRrLSh_nryhQ9w3-csJrQMWVic05wcROQupVdKvOd04Sx7tJC9Xh8XN-JA',
      imageAlt: 'Space probe in deep space',
      wide: true, tags: ['Quan Sát & Kính Thiên Văn'], date: '1977 - 2025'
    }
  ];

  archiveItems: ArchiveItem[] = [
    {
      code: 'B33', category: 'Chòm sao Lạp Hộ', distance: '1,375 LY',
      title: 'Tinh vân Đầu Ngựa (Barnard 33)',
      description: 'Đám mây bụi tối sẫm có hình dạng biểu tượng trỗi dậy trước bức xạ rực lửa của ngôi sao Sigma Orionis, che chắn vườn ươm sao bên trong.',
      classification: 'Tinh vân hấp thụ',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCfxHPC3kriEz5Y8Vp8geDPmAoQ8GCE2PnysmYSRMijmwSNkdyUoOkeRXhUJSbZ593fqR7thkvFsxxwQ_SbH_Bifbl9nBimU3QwvZFQSC5lssGdtia3ll7uGAKDOq5iKB2hcF8QctcOuJQIEK-D2mkksLK13NDGP0APFro5N3PCjxs8rwV2BvgtYS-gDkm7QeopJxuZdpJCGK-wwlB8iX4RQU32uAup4_uXuOFDw1aur3rjAP8Pg5pnCEf6kNTsTLicRMm76SRm_A',
      imageAlt: 'Horsehead Nebula'
    },
    {
      code: 'Alpha CMa', category: 'Chòm sao Đại Khuyển', distance: '8.6 LY',
      title: 'Hệ sao đôi Sirius & Ngọn Hải Đăng Trắng',
      description: 'Ngôi sao sáng nhất trên bầu trời đêm của Trái Đất cùng người bạn đồng hành "Chú Chó Con" Sirius B — một sao lùn trắng cô đặc có khối lượng bằng Mặt Trời.',
      classification: 'Cấp sao biểu kiến: -1.46',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBTINov1p8VumHw3X4NDPmWDoZjNit4NdSl3DrtsNYuZoe1A25h9eevaVlo_E14dk89R08sC7L9K90978Cel2P_Jr-_wsFgNuFKfnasYSF2hhQoGDc1LaCHWIAS0VbzvaoKJgJ8g6-lPGiSeKro27QztWqPvnHLX9ZDyIPiDLxaZ46sVCGijyhP2w8LUuv1zD4P8cO33g2nJ908ntKOCVWLGqLL8P0OHi4l5p6HGjREemI-MbIH5bIjgDi34DMiiWYdULwwOPCZMQ',
      imageAlt: 'Sirius binary star system'
    },
    {
      code: 'Mục tiêu sinh học', category: 'Vệ tinh Sao Mộc', distance: '628 Triệu km',
      title: 'Mặt trăng Europa & Đại dương Ngầm',
      description: 'Bí ẩn đại dương nước lỏng khổng lồ có thể tích gấp đôi mọi đại dương Trái Đất ẩn mình bên dưới lớp vỏ băng vĩnh cửu dày 20km.',
      classification: 'Nhiệm vụ: Europa Clipper',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuC6FdoK9WWwgOQ9cxMWDoq0BKn6XespN1KKaYpDHvn2UOMcMWKdeuxCEkGWYqnqiqk9ovPJlEJobeZUWUM2Bp_knFysH4PSPhLUeeP0_seCsqsR9VZc0QO8aUIGALhZJECCtvubvDlMZej5nOOsg3QobmRooqOMGEP-5D5W8bO6rk1sQKrxCHwzjCagDd5j8DYwf-6qaEDOmK3_STgnaUZdgoA7HSl33ywnOWsWFfRiXEc5w97uPY3k1EiySDSRuqKm_JEI0mIgg',
      imageAlt: "Jupiter's moon Europa"
    }
  ];

  storeItems: StoreItem[] = [
    {
      badge: 'Niên đại 4.5 tỷ năm', origin: 'Mẫu vật tự nhiên • Namibia',
      title: 'Mảnh Thiên thạch Gibeon Cổ đại',
      description: 'Hợp kim sắt-niken nguyên thủy từ lõi một tiểu hành tinh bị phá hủy khi Hệ Mặt Trời mới hình thành, sở hữu hoa văn Widmanstätten đặc trưng.',
      price: '3,850,000 ₫',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDh8dJS7jizEhHHvRNazFCTRLUAFi3_k-h7pW0kq96hZtQWg6WgkF02UXolUkkDLmJgdQgkgU6ZKqeCaHZWaM6qW2CVD_DJR3nl-qBGCs8HX3_ylNT89PFOiAfBFCwBSv8kOE0vF-pJ7AFsVhqOum0P4x5xbML_YJM00N9xj4IB3Ul5kctZ81txvSQ6_JTqQPUpAERyD7iIiTefA88p9c-nZF6uRDOhvwNL2zgt8ts5Q8j0ddOpJR1YizfJ4HVPOHS9T82NiVIXwQ',
      imageAlt: 'Gibeon meteorite fragment'
    },
    {
      badge: 'Ấn bản giới hạn', origin: 'Tài liệu phục chế • Ý (1610)',
      title: 'Bản đồ Sao Thiên Văn Cổ Galileo',
      description: 'Giấy da thủ công viền chỉ vàng, tái hiện chính xác các bản vẽ tay đầu tiên của Galileo Galilei khi lần đầu nhìn thấy 4 mặt trăng sao Mộc.',
      price: '1,250,000 ₫',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDRFE5fIdqGYqwXaJWMerGkoBzP7baLSmrVhuQ881_TT0ViEaoss5KqH-q632ZhX2-P3XwNVNI1dRE5zNBfI8FVtTeLr6812lki8G6c2JyaNyqh68KjA03Db5NRvtPKtyZjC776i0MA6-EhG-QeI526v9WxsXfpXZxqSFqFeOYavopls0D5yqMcxvnzhX1QI5lTfOA3O-6MsGfCT8bxW9E1WXIFWic7L3ojpJ35iKwIVGuT2ymIY3tOYOcOJmbG2xXB8fWoe3jsuQ',
      imageAlt: 'Antique celestial star chart'
    },
    {
      badge: 'Chế tác thủ công', origin: 'Công cụ hàng hải • Thế kỷ 16',
      title: 'Thước Trắc Tinh Đồng Thau (Astrolabe)',
      description: 'Thiết bị định vị thiên văn cổ điển được đúc từ đồng thau nguyên khối với vòng khắc độ chính xác cao giúp xác định vị trí các chòm sao chính.',
      price: '5,400,000 ₫',
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuD-0g5AoVslowz10VvXBpfOvDYU8RjC2WVrS0tHoDM7cUkzmgexdtGpnkypK1rjM7fX5-NwENPJSMHr-mPudNqGe1VDVdTerOia7NMDAjwPnxw3-ApdhuYoF2TfyyuyMgIn7ekZAa97pTnMefTmBeqNJ7cQQUZobvb1h2caDF7Blbpnv9aKumjw4Zr2qtYmR3XqaFLyvamMcJmjeccrqGRsL-rC9TDC98Q7uRNLMNGwgKXQ-rIFOXDLUa9k_qWET4QSKNlO5fcvRw',
      imageAlt: 'Vintage brass astrolabe'
    }
  ];

  cartItems: { name: string; price: string }[] = [];

  addToCart(name: string, price: string) {
    this.cartItems.push({ name, price });
  }
}
