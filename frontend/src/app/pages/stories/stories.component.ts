import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StoriesService } from '../../features/stories/services/stories.service';
import { NewsletterService } from '../../features/newsletter/services/newsletter.service';

interface Story {
  id: string | number;
  slug: string;
  category: string;
  era: string;
  code: string;
  title: string;
  description: string;
  author: string;
  readTime: number;
  image: string;
  alt: string;
  content?: string;
}

interface StoryCategory {
  id: string;
  label: string;
  count: number;
}

@Component({
  selector: 'app-stories',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './stories.component.html',
  styleUrl: './stories.component.scss'
})
export class StoriesComponent {
  private readonly storiesApi = inject(StoriesService);
  private readonly newsletter = inject(NewsletterService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  // ============================================================
  // UI STATE
  // ============================================================

  selectedCategory = 'all';
  searchTerm = '';
  sortMode = 'newest';

  isModalOpen = false;

  isSubscribing = false;
  isSubscribed = false;
  subscriptionError = '';
  shareMessage = '';
  storiesError = '';

  selectedStory: Story | null = null;

  isLoading = false;
  private storiesLoaded = false;
  private pendingStorySlug: string | null = null;

  // ============================================================
  // CATEGORY DATA
  // ============================================================

  categories: StoryCategory[] = [];

  // ============================================================
  // HERO STORY
  // ============================================================

  heroStory: Story | null = null;

  // ============================================================
  // STORIES
  // ============================================================

  stories: Story[] = [];

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.syncStoryFromUrl());
    this.loadStories();
  }

  private loadStories(): void {
    this.isLoading = true;
    this.storiesApi.getStories().subscribe({
      next: result => {
        if (result.items.length > 0) {
          const mapped = result.items.map(story => this.mapStory(story));
          this.stories = mapped;
          this.heroStory = mapped[0];
          this.categories = [
            { id: 'all', label: 'Tất cả biên niên sử', count: result.totalCount },
            ...Array.from(new Set(mapped.map(story => story.category))).map(category => ({
              id: category,
              label: category,
              count: mapped.filter(story => story.category === category).length
            }))
          ];
        }
        this.storiesError = '';
        this.storiesLoaded = true;
        this.syncStoryFromUrl();
        this.isLoading = false;
      },
      error: () => {
        this.stories = [];
        this.heroStory = null;
        this.categories = [];
        this.storiesError = 'Không thể tải biên niên sử. Vui lòng thử lại sau.';
        this.storiesLoaded = true;
        this.syncStoryFromUrl();
        this.isLoading = false;
      }
    });
  }

  private mapStory(story: {
    id: string;
    slug: string;
    category: string;
    era: string;
    code: string;
    title: string;
    description: string;
    content: string;
    author: string;
    readTimeMinutes: number;
    imageUrl?: string | null;
    imageAlt?: string | null;
  }): Story {
    return {
      id: story.id,
      slug: story.slug,
      category: story.category,
      era: story.era,
      code: story.code,
      title: story.title,
      description: story.description,
      content: story.content,
      author: story.author,
      readTime: story.readTimeMinutes,
      image: story.imageUrl ?? 'assets/images/cosmic-placeholder.svg',
      alt: story.imageAlt ?? story.title
    };
  }

  // ============================================================
  // GET CATEGORY LABEL
  // ============================================================

  getCategoryLabel(category: string): string {
    const found = this.categories.find(item => item.id === category);
    return found?.label ?? category;
  }

  // ============================================================
  // FILTERED STORIES
  // ============================================================

  get filteredStories(): Story[] {
    let result = [...this.stories];

    if (this.selectedCategory !== 'all') {
      result = result.filter(
        story => story.category === this.selectedCategory
      );
    }

    const keyword = this.searchTerm.trim().toLowerCase();

    if (keyword) {
      result = result.filter(story =>
        story.title.toLowerCase().includes(keyword) ||
        story.description.toLowerCase().includes(keyword) ||
        story.author.toLowerCase().includes(keyword) ||
        story.era.toLowerCase().includes(keyword) ||
        story.code.toLowerCase().includes(keyword)
      );
    }

    switch (this.sortMode) {
      case 'shortest':
        return [...result].sort((a, b) => a.readTime - b.readTime);

      case 'duration':
        return [...result].sort((a, b) => b.readTime - a.readTime);

      case 'newest':
      default:
        return result;
    }
  }

  // ============================================================
  // CATEGORY
  // ============================================================

  selectCategory(category: string): void {
    this.selectedCategory = category;
  }

  // ============================================================
  // SEARCH
  // ============================================================

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm = input.value;
  }

  // ============================================================
  // SORT
  // ============================================================

  onSortChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.sortMode = select.value;
  }

  // ============================================================
  // MODAL
  // ============================================================

  openModal(story?: Story | null): void {
    if (!story) {
      return;
    }

    this.pendingStorySlug = story.slug;
    this.showModal(story);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { story: story.slug },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  private showModal(story: Story): void {
    this.selectedStory = story;
    this.isModalOpen = true;
    document.body.style.overflow = 'hidden';
  }

  private syncStoryFromUrl(): void {
    const storySlug = this.route.snapshot.queryParamMap.get('story');
    if (!storySlug) {
      this.pendingStorySlug = null;
      if (this.isModalOpen) this.hideModal();
      return;
    }

    if (!this.storiesLoaded) return;
    if (storySlug === this.pendingStorySlug) return;
    this.pendingStorySlug = storySlug;

    const story = this.stories.find(item => item.slug === storySlug);
    if (story) {
      this.showModal(story);
    } else if (this.storiesLoaded) {
      this.storiesApi.getBySlug(storySlug).subscribe({
        next: result => {
          if (this.route.snapshot.queryParamMap.get('story') === storySlug) {
            this.showModal(this.mapStory(result));
          }
        },
        error: () => {
          if (this.route.snapshot.queryParamMap.get('story') !== storySlug) return;
          this.storiesError = 'Không tìm thấy biên niên sử được chia sẻ.';
          this.clearStoryQuery();
        }
      });
    }
  }

  closeModal(): void {
    this.hideModal();
    if (this.route.snapshot.queryParamMap.has('story')) {
      this.clearStoryQuery();
    }
  }

  private clearStoryQuery(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { story: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  private hideModal(): void {
    this.isModalOpen = false;
    this.selectedStory = null;
    document.body.style.overflow = 'auto';
  }

  async shareStory(): Promise<void> {
    if (!this.selectedStory) return;

    this.shareMessage = '';
    const url = new URL(
      `/stories?story=${encodeURIComponent(this.selectedStory.slug)}`,
      window.location.origin
    ).toString();
    const shareData = {
      title: this.selectedStory.title,
      text: this.selectedStory.description,
      url
    };

    try {
      if (navigator.share) {
        await navigator.share(shareData);
        this.shareMessage = 'Đã mở tùy chọn chia sẻ.';
      } else {
        await navigator.clipboard.writeText(url);
        this.shareMessage = 'Đã sao chép liên kết bài viết.';
      }
    } catch {
      this.shareMessage = 'Không thể chia sẻ bài viết trên thiết bị này.';
    }
  }

  // ============================================================
  // NEWSLETTER
  // ============================================================

  subscribe(email: string): void {
    if (!email || this.isSubscribing) {
      return;
    }

    this.isSubscribing = true;
    this.isSubscribed = false;
    this.subscriptionError = '';
    this.newsletter.subscribe(email).subscribe({
      next: () => {
        this.isSubscribing = false;
        this.isSubscribed = true;
        setTimeout(() => this.isSubscribed = false, 4700);
      },
      error: () => {
        this.isSubscribing = false;
        this.subscriptionError = 'Không thể đăng ký nhận tin. Vui lòng thử lại.';
      }
    });
  }

  // ============================================================
  // MODAL BACKDROP
  // ============================================================

  onModalBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeModal();
    }
  }

  // ============================================================
  // KEYBOARD
  // ============================================================

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isModalOpen) {
      this.closeModal();
    }
  }

  // ============================================================
  // CLEANUP
  // ============================================================

  ngOnDestroy(): void {
    document.body.style.overflow = 'auto';
  }
}