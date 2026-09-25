import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { StoriesService } from '../../features/stories/services/stories.service';
import { NewsletterService } from '../../features/newsletter/services/newsletter.service';

interface Story {
  id: string | number;
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

  // ============================================================
  // UI STATE
  // ============================================================

  selectedCategory = 'all';
  searchTerm = '';
  sortMode = 'newest';

  isModalOpen = false;
  isHeroAudioPlaying = false;
  isModalAudioPlaying = false;
  isAmbientOn = false;

  isSubscribing = false;
  isSubscribed = false;

  selectedStory: Story | null = null;

  isLoading = false;

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
        this.isLoading = false;
      },
      error: () => {
        this.stories = [];
        this.heroStory = null;
        this.categories = [];
        this.isLoading = false;
      }
    });
  }

  private mapStory(story: {
    id: string;
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
      case 'popular':
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

    this.selectedStory = story;
    this.isModalOpen = true;
    this.isModalAudioPlaying = false;

    document.body.style.overflow = 'hidden';
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.isModalAudioPlaying = false;

    document.body.style.overflow = 'auto';
  }

  // ============================================================
  // AUDIO
  // ============================================================

  toggleHeroAudio(): void {
    this.isHeroAudioPlaying = !this.isHeroAudioPlaying;
  }

  toggleModalAudio(): void {
    this.isModalAudioPlaying = !this.isModalAudioPlaying;
  }

  toggleAmbient(): void {
    this.isAmbientOn = !this.isAmbientOn;
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
    this.newsletter.subscribe(email).subscribe({
      next: () => {
        this.isSubscribing = false;
        this.isSubscribed = true;
        setTimeout(() => this.isSubscribed = false, 4700);
      },
      error: () => {
        this.isSubscribing = false;
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