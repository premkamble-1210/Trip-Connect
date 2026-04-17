import { Injectable } from '@angular/core';

export interface User {
  id: number;
  name: string;
  handle: string;
  rating: number;
  reviews: number;
  memberSince: string;
  email: string;
  phone: string;
  avatar: string;
  badges: Badge[];
}

export interface Badge {
  id: number;
  name: string;
  verified: boolean;
}

export interface Trip {
  id: number;
  title: string;
  location: string;
  dates: string;
  budget: string;
  seats: string;
  status: 'Upcoming' | 'Completed' | 'Live';
  organizerName: string;
  organizerAvatar: string;
}

export interface Review {
  id: number;
  reviewerName: string;
  reviewerAvatar: string;
  reviewDate: string;
  rating: number;
  tripName: string;
  reviewText: string;
  helpfulCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  
  getUserProfile(): User {
    return {
      id: 1,
      name: 'Sarah Peterson',
      handle: '@sarahp',
      rating: 4.9,
      reviews: 12,
      memberSince: 'January 2023',
      email: 's***p@gmail.com',
      phone: '***-***-5555',
      avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80',
      badges: [
        { id: 1, name: 'Phone', verified: true },
        { id: 2, name: 'ID', verified: false }
      ]
    };
  }

  getHostedTrips(): Trip[] {
    return [
      {
        id: 1,
        title: 'Explore Bali: Culture & Coastline',
        location: 'Bali, Indonesia',
        dates: 'Oct 12-20',
        budget: '$1,400',
        seats: '4/6 available',
        status: 'Upcoming',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 2,
        title: 'Alps Hiking Adventure',
        location: 'Interlaken, Switzerland',
        dates: 'Sep 18-25',
        budget: '$900',
        seats: '1/4 available',
        status: 'Upcoming',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 3,
        title: 'New York City Explorer',
        location: 'New York City, USA',
        dates: 'Aug 5-10',
        budget: '$1,200',
        seats: '6/6 full',
        status: 'Completed',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 4,
        title: 'Thailand Beach Paradise',
        location: 'Phuket, Thailand',
        dates: 'Nov 2-9',
        budget: '$850',
        seats: '3/5 available',
        status: 'Upcoming',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 5,
        title: 'Iceland Northern Lights Quest',
        location: 'Reykjavik, Iceland',
        dates: 'Dec 10-17',
        budget: '$1,600',
        seats: '2/4 available',
        status: 'Upcoming',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 6,
        title: 'Portugal Wine & Coast Tour',
        location: 'Algarve, Portugal',
        dates: 'May 15-22',
        budget: '$950',
        seats: '5/6 available',
        status: 'Upcoming',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 7,
        title: 'Mexico City Cultural Immersion',
        location: 'Mexico City, Mexico',
        dates: 'Jul 1-8',
        budget: '$1,100',
        seats: '4/4 full',
        status: 'Completed',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 8,
        title: 'Greece Island Hopping',
        location: 'Santorini, Greece',
        dates: 'Jun 5-15',
        budget: '$1,350',
        seats: '2/5 available',
        status: 'Upcoming',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 9,
        title: 'Canada Rocky Mountains Trek',
        location: 'Banff, Canada',
        dates: 'Sep 8-16',
        budget: '$1,050',
        seats: '3/4 available',
        status: 'Completed',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      }
    ];
  }

  getJoinedTrips(): Trip[] {
    return [
      {
        id: 1,
        title: 'Alps Hiking Adventure',
        location: 'Interlaken, Switzerland',
        dates: 'Sep 18-25',
        budget: '$2,100',
        seats: '1/4 available',
        status: 'Live',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 2,
        title: 'New York City Explorer',
        location: 'New York City, USA',
        dates: 'Aug 5-10',
        budget: '$1,300',
        seats: '6/6 full',
        status: 'Completed',
        organizerName: 'Sarah Peterson',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 3,
        title: 'Tokyo Urban Adventure',
        location: 'Tokyo, Japan',
        dates: 'Mar 10-20',
        budget: '$2,200',
        seats: '3/5 available',
        status: 'Upcoming',
        organizerName: 'Marcus Chen',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 4,
        title: 'Dubai Desert & Luxury',
        location: 'Dubai, UAE',
        dates: 'Feb 14-21',
        budget: '$1,800',
        seats: '2/4 available',
        status: 'Completed',
        organizerName: 'Aisha Khan',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 5,
        title: 'Bali Wellness Retreat',
        location: 'Bali, Indonesia',
        dates: 'Jan 5-12',
        budget: '$950',
        seats: '4/6 available',
        status: 'Completed',
        organizerName: 'Lisa Wong',
        organizerAvatar: 'https://via.placeholder.com/32'
      },
      {
        id: 6,
        title: 'Barcelona Food & Culture',
        location: 'Barcelona, Spain',
        dates: 'Apr 20-26',
        budget: '$1,150',
        seats: '5/5 full',
        status: 'Upcoming',
        organizerName: 'Carlos Gonzalez',
        organizerAvatar: 'https://via.placeholder.com/32'
      }
    ];
  }

  getReviews(): Review[] {
    return [
      {
        id: 1,
        reviewerName: 'John Smith',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Aug 15, 2024',
        rating: 5,
        tripName: 'Trip: New York City Explorer',
        reviewText: 'Sarah was an amazing trip organizer! She planned everything to perfection, from accommodations to activities. The entire group had an unforgettable experience. Highly recommend joining any trip she organizes!',
        helpfulCount: 8
      },
      {
        id: 2,
        reviewerName: 'Emily Davis',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Jul 22, 2024',
        rating: 4.5,
        tripName: 'Trip: Paris Weekend Getaway',
        reviewText: 'Great organizer with good communication. The itinerary was well-structured. Would have appreciated more flexibility with free time, but overall a wonderful experience!',
        helpfulCount: 5
      },
      {
        id: 3,
        reviewerName: 'Michael Brown',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Jun 10, 2024',
        rating: 5,
        tripName: 'Trip: Japan Cultural Tour',
        reviewText: 'Exceptional organization and attention to detail. Sarah ensured every member felt welcome and included in the group. The cultural experiences she arranged were authentic and enriching. Will definitely join her next trip!',
        helpfulCount: 12
      },
      {
        id: 4,
        reviewerName: 'Sophie Laurent',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on May 18, 2024',
        rating: 5,
        tripName: 'Trip: Italy Renaissance Journey',
        reviewText: 'Absolutely phenomenal experience! Sarah\'s knowledge of Italian history and culture was impressive. She made sure everyone felt included and safe throughout the trip. The guide she hired was excellent. 10/10!',
        helpfulCount: 15
      },
      {
        id: 5,
        reviewerName: 'David Martinez',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Apr 3, 2024',
        rating: 4,
        tripName: 'Trip: Scotland Highlands Explorer',
        reviewText: 'Really enjoyed the trip! Sarah handled logistics well and the group dynamics were wonderful. The accommodation could have been a bit more comfortable, but the scenic views and activities made up for it.',
        helpfulCount: 7
      },
      {
        id: 6,
        reviewerName: 'Jennifer Lee',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Mar 15, 2024',
        rating: 5,
        tripName: 'Trip: Thailand Island Hopping',
        reviewText: 'Best travel experience I\'ve had! Sarah\'s attention to every detail was remarkable. She found hidden gems that tourists typically miss. The entire group bonded beautifully. Looking forward to her next adventure!',
        helpfulCount: 18
      },
      {
        id: 7,
        reviewerName: 'Thomas Wright',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Feb 28, 2024',
        rating: 4.5,
        tripName: 'Trip: Australian Outback Safari',
        reviewText: 'Excellent trip planning and execution. Sarah was very responsive to any concerns. The wildlife experiences were incredible. Minor issue with one accommodation, but Sarah handled it professionally.',
        helpfulCount: 10
      },
      {
        id: 8,
        reviewerName: 'Olivia Chen',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Jan 20, 2024',
        rating: 5,
        tripName: 'Trip: Peru Machu Picchu Trek',
        reviewText: 'Sarah made our dream trip come true! Her expertise on the region, the physical preparation guidance, and constant support made the trek manageable and memorable. Genuinely one of the best experiences of my life.',
        helpfulCount: 22
      },
      {
        id: 9,
        reviewerName: 'Nathan Green',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Dec 10, 2023',
        rating: 4,
        tripName: 'Trip: Nordic Countries Adventure',
        reviewText: 'Great organization and plenty of activities. Sarah was accommodating with special requests. The only minor downside was the tight schedule didn\'t allow for much spontaneity, but overall excellent trip!',
        helpfulCount: 9
      },
      {
        id: 10,
        reviewerName: 'Sophia Rodriguez',
        reviewerAvatar: 'https://via.placeholder.com/48',
        reviewDate: 'Reviewed on Nov 5, 2023',
        rating: 5,
        tripName: 'Trip: Egypt Ancient Wonders',
        reviewText: 'Sarah\'s passion for travel is contagious! She paired educational experiences with pure fun. Her local connections helped us access exclusive sites and experiences. Highly recommended to anyone considering a group trip!',
        helpfulCount: 19
      }
    ];
  }
}
