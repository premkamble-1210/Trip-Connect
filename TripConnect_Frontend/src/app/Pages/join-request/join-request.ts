import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface JoinRequestItem {
  id: number;
  name: string;
  username: string;
  avatar: string;
  status: 'Pending' | 'Accepted' | 'Rejected' | 'Cancelled';
}

export interface FilterOption {
  label: string;
  value: string;
  selected: boolean;
}

@Component({
  selector: 'app-join-request',
  imports: [CommonModule],
  templateUrl: './join-request.html',
  styleUrl: './join-request.css',
})
export class JoinRequest {
  tripName: string = 'Bali Yoga Retreat';
  
  filters: FilterOption[] = [
    { label: 'Pending', value: 'Pending', selected: true },
    { label: 'Accepted', value: 'Accepted', selected: false },
    { label: 'Rejected', value: 'Rejected', selected: false },
    { label: 'Cancelled', value: 'Cancelled', selected: false },
  ];

  requests: JoinRequestItem[] = [
    {
      id: 1,
      name: 'John Doe',
      username: 'annaelrunarise',
      avatar: 'https://randomuser.me/api/portraits/men/1.jpg',
      status: 'Pending',
    },
    {
      id: 2,
      name: 'Emily Chen',
      username: 'nemotsnowneo',
      avatar: 'https://randomuser.me/api/portraits/women/2.jpg',
      status: 'Pending',
    },
    {
      id: 3,
      name: 'Michael Brown',
      username: 'numedrunansio',
      avatar: 'https://randomuser.me/api/portraits/men/3.jpg',
      status: 'Accepted',
    },
    {
      id: 4,
      name: 'Lisa Wang',
      username: 'lisa.wang@email.com',
      avatar: 'https://randomuser.me/api/portraits/women/4.jpg',
      status: 'Pending',
    },
    {
      id: 5,
      name: 'David Smith',
      username: 'davidsmith92',
      avatar: 'https://randomuser.me/api/portraits/men/5.jpg',
      status: 'Rejected',
    },
    {
      id: 6,
      name: 'Sarah Johnson',
      username: 'sarahj_travels',
      avatar: 'https://randomuser.me/api/portraits/women/6.jpg',
      status: 'Cancelled',
    },
    {
      id: 7,
      name: 'James Wilson',
      username: 'jameswilson_',
      avatar: 'https://randomuser.me/api/portraits/men/7.jpg',
      status: 'Pending',
    },
    {
      id: 8,
      name: 'Emma Thompson',
      username: 'emma.t.travels',
      avatar: 'https://randomuser.me/api/portraits/women/8.jpg',
      status: 'Accepted',
    },
    {
      id: 9,
      name: 'Robert Garcia',
      username: 'robgarcia',
      avatar: 'https://randomuser.me/api/portraits/men/9.jpg',
      status: 'Pending',
    },
    {
      id: 10,
      name: 'Olivia Martinez',
      username: 'olivia_m',
      avatar: 'https://randomuser.me/api/portraits/women/10.jpg',
      status: 'Accepted',
    },
    {
      id: 11,
      name: 'William Anderson',
      username: 'will.anderson',
      avatar: 'https://randomuser.me/api/portraits/men/11.jpg',
      status: 'Rejected',
    },
    {
      id: 12,
      name: 'Sophia Taylor',
      username: 'sophiataylor99',
      avatar: 'https://randomuser.me/api/portraits/women/12.jpg',
      status: 'Pending',
    },
    {
      id: 13,
      name: 'Daniel Lee',
      username: 'danlee_explorer',
      avatar: 'https://randomuser.me/api/portraits/men/13.jpg',
      status: 'Cancelled',
    },
    {
      id: 14,
      name: 'Ava Robinson',
      username: 'ava.r.adventures',
      avatar: 'https://randomuser.me/api/portraits/women/14.jpg',
      status: 'Accepted',
    },
    {
      id: 15,
      name: 'Christopher White',
      username: 'chris_white',
      avatar: 'https://randomuser.me/api/portraits/men/15.jpg',
      status: 'Pending',
    },
    {
      id: 16,
      name: 'Isabella Harris',
      username: 'bella_harris',
      avatar: 'https://randomuser.me/api/portraits/women/16.jpg',
      status: 'Rejected',
    },
    {
      id: 17,
      name: 'Matthew Clark',
      username: 'matt.clark',
      avatar: 'https://randomuser.me/api/portraits/men/17.jpg',
      status: 'Accepted',
    },
    {
      id: 18,
      name: 'Mia Lewis',
      username: 'mia_travels',
      avatar: 'https://randomuser.me/api/portraits/women/18.jpg',
      status: 'Cancelled',
    },
  ];

  profile = {
    name: 'Sarah P.',
    username: '@teamawesome',
    avatar: 'https://randomuser.me/api/portraits/women/1.jpg',
  };

  selectedFilter: string = 'Pending';

  get pendingCount(): number {
    return this.requests.filter(r => r.status === 'Pending').length;
  }

  get acceptedCount(): number {
    return this.requests.filter(r => r.status === 'Accepted').length;
  }

  get rejectedCount(): number {
    return this.requests.filter(r => r.status === 'Rejected').length;
  }

  get cancelledCount(): number {
    return this.requests.filter(r => r.status === 'Cancelled').length;
  }

  get totalCount(): number {
    return this.requests.length;
  }

  get filteredRequests(): JoinRequestItem[] {
    return this.requests.filter(r => r.status === this.selectedFilter);
  }

  selectFilter(filter: FilterOption): void {
    this.filters.forEach(f => f.selected = false);
    filter.selected = true;
    this.selectedFilter = filter.value;
  }

  acceptRequest(request: JoinRequestItem): void {
    request.status = 'Accepted';
  }

  declineRequest(request: JoinRequestItem): void {
    request.status = 'Rejected';
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'bg-orange-100 text-orange-600';
      case 'Accepted':
        return 'bg-green-100 text-green-600';
      case 'Rejected':
        return 'bg-red-100 text-red-600';
      case 'Cancelled':
        return 'bg-gray-100 text-gray-600';
      default:
        return 'bg-gray-100 text-gray-600';
    }
  }
}
